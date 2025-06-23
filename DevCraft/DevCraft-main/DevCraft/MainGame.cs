using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using DevCraft.World;
using DevCraft.Utilities;
using DevCraft.Assets;
using DevCraft.World.Blocks;
using DevCraft.GUI.Menus;
using DevCraft.GUI.Core;
using DevCraft.Persistence;
using DevCraft.Rendering;
using DevCraft.Rendering.Meshers;
using DevCraft.World.Chunks;

namespace DevCraft
{
    public enum GameState
    {
        Loading,
        Running,
        MainMenu,
        Exiting
    }

    public class MainGame : Game
    {
        public GameState State { get; set; }
        public bool Paused { get; set; }
        public bool ExitedMenu { get; set; }

        GraphicsDeviceManager graphics;

        readonly string assetDirectory;

        Player player;
        readonly AssetServer assetServer;
        readonly BlockMetadataProvider blockMetadata;
        WorldSystem world;
        Renderer renderer;
        GameMenu gameMenu;
        MainMenu mainMenu;
        FilePersistenceService persistenceService;
        Save currentSave;
        Time time;

        // Modern UI management systems
        UIStateManager uiStateManager;
        UIAnimationManager uiAnimationManager;
        UIInputManager uiInputManager;


        public MainGame()
        {
            // Set the correct asset directory path
            assetDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
            
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720,
                IsFullScreen = false,
                SynchronizeWithVerticalRetrace = false, // Disable VSync for higher frame rates
                PreferMultiSampling = true, // Enable anti-aliasing to reduce edge artifacts
                PreferredDepthStencilFormat = DepthFormat.Depth24, // Better depth precision
                PreferredBackBufferFormat = SurfaceFormat.Color // Ensure proper color format
            };

            // Enable higher frame rates with 1000 FPS cap
            IsFixedTimeStep = false;
            TargetElapsedTime = TimeSpan.FromMilliseconds(1.0); // 1000 FPS cap

            Content.RootDirectory = "Assets";

            blockMetadata = new BlockMetadataProvider(assetDirectory);
            assetServer = new AssetServer(Content, "Assets");
        }

        public void ApplyDisplayMode()
        {
            // Ensure monitor index is valid
            int monitorCount = MonitorHelper.GetMonitorCount();
            if (Settings.MonitorIndex >= monitorCount)
                Settings.MonitorIndex = 0;

            var selectedMonitor = MonitorHelper.GetMonitor(Settings.MonitorIndex);
            
            switch (Settings.DisplayMode)
            {
                case "Fullscreen":
                    graphics.GraphicsProfile = GraphicsProfile.Reach;
                    graphics.IsFullScreen = true;
                    Window.IsBorderless = false;
                    graphics.PreferredBackBufferWidth = selectedMonitor.Width;
                    graphics.PreferredBackBufferHeight = selectedMonitor.Height;
                    break;
                case "Borderless":
                    graphics.IsFullScreen = false;
                    Window.IsBorderless = true;
                    graphics.PreferredBackBufferWidth = selectedMonitor.Width;
                    graphics.PreferredBackBufferHeight = selectedMonitor.Height;
                    break;
                case "Windowed":
                default:
                    graphics.IsFullScreen = false;
                    Window.IsBorderless = false;
                    graphics.PreferredBackBufferWidth = 1280;
                    graphics.PreferredBackBufferHeight = 720;
                    // Center windowed mode on selected monitor - apply after graphics changes
                    break;
            }
            graphics.ApplyChanges();
            
            // Position window after graphics changes are applied
            if (Settings.DisplayMode == "Windowed")
            {
                var centerPoint = MonitorHelper.GetCenterPointForMonitor(Settings.MonitorIndex, 1280, 720);
                Window.Position = new Microsoft.Xna.Framework.Point(centerPoint.X, centerPoint.Y);
            }
            else if (Settings.DisplayMode == "Borderless")
            {
                // Position borderless window on selected monitor
                Window.Position = new Microsoft.Xna.Framework.Point(selectedMonitor.Left, selectedMonitor.Top);
            }
            
            // Refresh the main menu layout if it exists
            if (mainMenu != null)
            {
                mainMenu.RefreshLayout(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            }
        }

        public static string GetMonitorName(int index)
        {
            return MonitorHelper.GetMonitorDisplayName(index);
        }

        public static int GetMonitorCount()
        {
            return MonitorHelper.GetMonitorCount();
        }

        protected override void Initialize()
        {
            State = GameState.MainMenu;
            Paused = false;
            ExitedMenu = false;

            // Ensure proper graphics quality and color rendering
            graphics.GraphicsProfile = GraphicsProfile.Reach;
            
            Settings.Load();
            ApplyDisplayMode();

            blockMetadata.Load();
            assetServer.Load(GraphicsDevice);

            // Initialize UI management systems
            uiStateManager = new UIStateManager();
            uiAnimationManager = new UIAnimationManager();
            uiInputManager = new UIInputManager();

            // Set up UI state events
            uiStateManager.OnStateChanged += (prev, current) => 
            {
                // Handle state transitions
                IsMouseVisible = current == UIStateManager.UIState.MainMenu || 
                                current == UIStateManager.UIState.Settings ||
                                current == UIStateManager.UIState.Inventory ||
                                current == UIStateManager.UIState.Paused;
            };

            uiStateManager.OnError += error => System.Console.WriteLine($"UI Error: {error}");

            mainMenu = new MainMenu(this, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, GraphicsDevice, assetServer);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            
            // Now that graphics device is ready, position the window correctly
            CenterWindowOnSelectedMonitor();
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                // Update UI management systems
                uiInputManager.Update();
                uiAnimationManager.Update(gameTime);

                switch (State)
                {
                    case GameState.Running:
                        {
                            if (!Paused)
                            {
                                world.Update(gameTime, ExitedMenu);
                            }

                            gameMenu.Update();

                            break;
                        }

                    case GameState.Loading:
                        {
                            State = GameState.Running;

                            currentSave = mainMenu.CurrentSave;

                            time = new Time(currentSave.Parameters.Day, currentSave.Parameters.Hour, currentSave.Parameters.Minute);

                            ScreenshotTaker screenshotTaker = new(GraphicsDevice, Window.ClientBounds.Width,
                                                                                  Window.ClientBounds.Height);

                            persistenceService = new FilePersistenceService(this, currentSave.Parameters.SaveName, blockMetadata);
                            persistenceService.Initialize();

                            Region region = new(Settings.RenderDistance);

                            ChunkMesher chunkMesher = new(blockMetadata, assetServer);
                            BlockOutlineMesher blockOutlineMesher = new();

                            player = new Player(GraphicsDevice, currentSave.Parameters);
                            gameMenu = new GameMenu(this, GraphicsDevice, time, screenshotTaker, currentSave.Parameters, assetServer, blockMetadata, player);
                            world = new WorldSystem(region, gameMenu, persistenceService, currentSave.Parameters, blockMetadata, chunkMesher, blockOutlineMesher);
                            renderer = new Renderer(region, graphics.GraphicsDevice, assetServer, screenshotTaker, chunkMesher, blockOutlineMesher, blockMetadata);

                            world.Init(player, currentSave.Parameters);

                            if (!File.Exists($@"Saves/{currentSave.Parameters.SaveName}/save_icon.png"))
                            {
                                player.Update(gameTime);
                                renderer.Render(player.Camera, time, player, false);
                                screenshotTaker.SaveIcon(currentSave.Parameters.SaveName, out currentSave.Icon);
                            }

                            break;
                        }

                    case GameState.Exiting:
                        {
                            persistenceService?.Dispose();

                            player.SaveParameters(currentSave.Parameters);
                            time.SaveParameters(currentSave.Parameters);
                            currentSave.Parameters.Save();

                            world.Dispose();

                            player = null;
                            world = null;
                            persistenceService = null;
                            gameMenu = null;

                            State = GameState.MainMenu;
                            IsMouseVisible = true;

                            break;
                        }

                    case GameState.MainMenu:
                        {
                            mainMenu.Update();
                            break;
                        }
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            switch (State)
            {
                case GameState.Running:
                    {
                        renderer.Render(player.Camera, time, player, gameMenu.ShowHitboxes);
                        gameMenu.Draw((int)Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds));
                        break;
                    }

                case GameState.Loading:
                    {
                        mainMenu.DrawLoadingScreen();
                        break;
                    }

                case GameState.Exiting:
                    {
                        mainMenu.DrawSavingScreen();
                        break;
                    }

                case GameState.MainMenu:
                    {
                        mainMenu.Draw();
                        break;
                    }
            }

            base.Draw(gameTime);
        }

        private void CenterWindowOnSelectedMonitor()
        {
            try
            {
                // Debug: Print monitor information
                var allMonitors = MonitorHelper.GetAllMonitors();
                // Console.WriteLine($"Total monitors detected: {allMonitors.Count}");
                // for (int i = 0; i < allMonitors.Count; i++)
                // {
                //     var monitor = allMonitors[i];
                //     Console.WriteLine($"Monitor {i}: {monitor.FriendlyName}");
                //     Console.WriteLine($"  Position: ({monitor.Left}, {monitor.Top})");
                //     Console.WriteLine($"  Size: {monitor.Width}x{monitor.Height}");
                //     Console.WriteLine($"  Primary: {monitor.IsPrimary}");
                // }

                // Ensure monitor index is valid first
                int monitorCount = MonitorHelper.GetMonitorCount();
                if (Settings.MonitorIndex >= monitorCount || Settings.MonitorIndex < 0)
                {
                    Settings.MonitorIndex = 0;
                    // Find primary monitor index
                    for (int i = 0; i < allMonitors.Count; i++)
                    {
                        if (allMonitors[i].IsPrimary)
                        {
                            Settings.MonitorIndex = i;
                            break;
                        }
                    }
                }

                // Console.WriteLine($"Selected monitor index: {Settings.MonitorIndex}");
                var selectedMonitor = allMonitors[Settings.MonitorIndex];
                // Console.WriteLine($"Selected monitor: {selectedMonitor.FriendlyName} at ({selectedMonitor.Left}, {selectedMonitor.Top})");

                // Only center if we're in windowed mode
                if (Settings.DisplayMode == "Windowed")
                {
                    var centerPoint = MonitorHelper.GetCenterPointForMonitor(Settings.MonitorIndex, 1280, 720);
                    // Console.WriteLine($"Calculated center point: ({centerPoint.X}, {centerPoint.Y})");
                    Window.Position = new Microsoft.Xna.Framework.Point(centerPoint.X, centerPoint.Y);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CenterWindowOnSelectedMonitor: {ex.Message}");
                // If there's any error, just leave the window at default position
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose UI management systems
                uiStateManager?.Dispose();
                uiAnimationManager?.Dispose();
                uiInputManager?.Dispose();
                
                // Dispose other resources
                mainMenu?.Dispose();
                gameMenu?.Dispose();
                world?.Dispose();
                persistenceService?.Dispose();
            }
            
            base.Dispose(disposing);
        }
    }
}
