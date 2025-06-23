using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using DevCraft.Utilities;
using DevCraft.Assets;
using DevCraft.World.Blocks;
using DevCraft.World.Chunks;
using DevCraft.GUI.Elements;
using DevCraft.GUI.Components;
using DevCraft.GUI.Utilities;
using DevCraft.Configuration;
using DevCraft.Persistence;
using DevCraft.MathUtilities;


namespace DevCraft.GUI.Menus
{
    class GameMenu : IDisposable
    {
        public ushort SelectedItem => inventory.SelectedItem;
        public bool ShowHitboxes => showHitboxes;

        MainGame game;
        GraphicsDevice graphics;
        SpriteBatch spriteBatch;
        readonly AssetServer assetServer;
        readonly Player player;

        Inventory inventory;
        Time time;
        ScreenshotTaker screenshotHandler;

        Rectangle crosshair;

        Rectangle screenShading;
        Texture2D screenShadingTexture;

        Button back, quit;

        SpriteFont font14, font24;

        MenuState state;

        enum MenuState
        {
            Main,
            Pause,
            Inventory
        }

        bool debugScreen;
        bool showHitboxes;

        KeyboardState previousKeyboardState;
        MouseState previousMouseState;

        int screenWidth, screenHeight;
        Vector2 screenCenter;


        public GameMenu(MainGame game, GraphicsDevice graphics, Time time,
            ScreenshotTaker screenshotTaker, Parameters parameters, AssetServer assetServer,
            BlockMetadataProvider blockMetadata, Player player)
        {
            this.game = game;
            this.graphics = graphics;
            spriteBatch = new SpriteBatch(graphics);
            this.assetServer = assetServer;
            this.player = player;

            this.time = time;
            screenshotHandler = screenshotTaker;

            state = MenuState.Main;
            debugScreen = false;

            font14 = assetServer.GetFont(0);
            font24 = assetServer.GetFont(1);

            inventory = new Inventory(game, spriteBatch, font14, parameters, assetServer, blockMetadata, () =>
            {
                Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);
                game.IsMouseVisible = false;
                game.Paused = false;
                game.ExitedMenu = true;
                state = MenuState.Main;
            });

            previousKeyboardState = Keyboard.GetState();
            previousMouseState = Mouse.GetState();

            screenWidth = game.Window.ClientBounds.Width;
            screenHeight = game.Window.ClientBounds.Height;

            screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);

            // Get responsive button dimensions
            Point screenSize = new Point(screenWidth, screenHeight);
            Point buttonSize = UILayoutHelper.GetButtonSize(screenSize);
            int buttonSpacing = UILayoutHelper.GetButtonSpacing(screenSize);
            int menuButtonTopMargin = UILayoutHelper.GetMenuButtonTopMargin(screenSize);

            back = new Button(graphics, spriteBatch, "Back to game", font14,
                UILayoutHelper.CenterHorizontally(screenWidth, buttonSize.X), 
                menuButtonTopMargin, 
                buttonSize.X, 
                buttonSize.Y,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    game.Paused = false;

                    Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);
                    game.IsMouseVisible = false;
                    game.ExitedMenu = true;

                    state = MenuState.Main;
                });

            quit = new Button(graphics, spriteBatch, "Save & Quit", font14,
                UILayoutHelper.CenterHorizontally(screenWidth, buttonSize.X), 
                menuButtonTopMargin + buttonSpacing + buttonSize.Y, 
                buttonSize.X, 
                buttonSize.Y,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    game.Paused = false;
                    game.IsMouseVisible = false;
                    inventory.SaveParameters(parameters);
                    game.State = GameState.Exiting;
                });


            int crosshairSize = UILayoutHelper.GetCrosshairSize(screenSize);
            crosshair = new Rectangle(
                UILayoutHelper.CenterHorizontally(screenWidth, crosshairSize), 
                UILayoutHelper.CenterVertically(screenHeight, crosshairSize), 
                crosshairSize, 
                crosshairSize);

            screenShading = new Rectangle(0, 0, screenWidth, screenHeight);
            screenShadingTexture = new Texture2D(graphics, screenWidth, screenHeight);

            Color[] darkBackGroundColor = new Color[screenWidth * game.Window.ClientBounds.Height];
            for (int i = 0; i < darkBackGroundColor.Length; i++)
                darkBackGroundColor[i] = new Color(Color.Black, UIConstants.General.ScreenShadingAlpha);

            screenShadingTexture.SetData(darkBackGroundColor);
        }

        public void Update()
        {
            MouseState currentMouseState = Mouse.GetState();
            KeyboardState currentKeyboardState = Keyboard.GetState();
            Point mouseLoc = new(currentMouseState.X, currentMouseState.Y);

            game.ExitedMenu = false;

            switch (state)
            {
                case MenuState.Main:
                    {
                        MainControl(currentKeyboardState);
                        inventory.UpdateHotbar(currentMouseState);
                        break;
                    }

                case MenuState.Pause:
                    {
                        PauseControl(currentKeyboardState, currentMouseState, mouseLoc);
                        break;
                    }

                case MenuState.Inventory:
                    {
                        inventory.Update(currentKeyboardState, previousKeyboardState,
                            currentMouseState, previousMouseState, mouseLoc);
                        break;
                    }
            }

            previousKeyboardState = currentKeyboardState;
            previousMouseState = currentMouseState;
        }

        public void Draw(int fps)
        {
            MouseState currentMouseState = Mouse.GetState();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            inventory.DrawHotbar();

            if (game.Paused)
            {
                spriteBatch.Draw(screenShadingTexture, screenShading, Color.White);
            }

            switch (state)
            {
                case MenuState.Main:
                    {
                        spriteBatch.Draw(assetServer.GetMenuTexture("crosshair"), crosshair, Color.White);
                        break;
                    }

                case MenuState.Pause:
                    {
                        back.Draw();
                        quit.Draw();
                        break;
                    }

                case MenuState.Inventory:
                    {
                        inventory.Draw(currentMouseState);
                        break;
                    }
            }

            if (debugScreen)
            {
                DrawDebugScreen(fps);
            }

            spriteBatch.End();

            graphics.DepthStencilState = DepthStencilState.Default;
        }

        void MainControl(KeyboardState currentKeyboardState)
        {
            if (Util.KeyPressed(Keys.Escape, currentKeyboardState, previousKeyboardState))
            {
                game.Paused = true;

                Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);
                game.IsMouseVisible = true;

                state = MenuState.Pause;
            }

            else if (Util.KeyPressed(Keys.E, currentKeyboardState, previousKeyboardState))
            {
                game.Paused = true;

                Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);
                game.IsMouseVisible = true;

                state = MenuState.Inventory;
            }

            else if (Util.KeyPressed(Keys.F2, currentKeyboardState, previousKeyboardState))
            {
                screenshotHandler.TakeScreenshot = true;
            }

            else if (Util.KeyPressed(Keys.F3, currentKeyboardState, previousKeyboardState))
            {
                if (currentKeyboardState.IsKeyDown(Keys.B))
                {
                    showHitboxes ^= true;
                }
                else
                {
                    debugScreen ^= true;
                }
            }
        }

        void PauseControl(KeyboardState currentKeyboardState, MouseState currentMouseState, Point mouseLoc)
        {
            bool leftClick = Util.LeftButtonClicked(currentMouseState, previousMouseState);

            if (Util.KeyPressed(Keys.Escape, currentKeyboardState, previousKeyboardState))
            {
                game.Paused = false;

                Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);
                game.IsMouseVisible = false;

                state = MenuState.Main;
            }

            back.Update(mouseLoc, leftClick);
            quit.Update(mouseLoc, leftClick);
        }

        void DrawDebugScreen(int fps)
        {
            // Calculate responsive debug text positioning
            int debugX = (int)(screenWidth * UIConstants.Debug.DebugTextLeftRatio);
            int debugY = (int)(screenHeight * UIConstants.Debug.DebugTextTopRatio);
            int lineSpacing = (int)(screenHeight * UIConstants.Debug.DebugLineSpacingRatio);
            
            spriteBatch.DrawString(font14, "FPS: " + fps.ToString(), new Vector2(debugX, debugY), Color.White);
            spriteBatch.DrawString(font14, $"Memory: {GC.GetTotalMemory(false) / (1024 * 1024)} Mb",
                new Vector2(debugX, debugY + lineSpacing), Color.White);
            spriteBatch.DrawString(font14, time.ToString(), new Vector2(debugX, debugY + 2 * lineSpacing), Color.White);
            spriteBatch.DrawString(font14, $"X: {player.Position.X: 0.00}, Y: {player.Position.Y: 0.00}, Z: {player.Position.Z: 0.00}", 
                new Vector2(debugX, debugY + 3 * lineSpacing), Color.White);
            Vec3<int> chunkIndex = Chunk.WorldToChunkCoords(player.Position);
            spriteBatch.DrawString(font14, $"CX: {chunkIndex.X}, CY: {chunkIndex.Y}, CZ: {chunkIndex.Z}", 
                new Vector2(debugX, debugY + 4 * lineSpacing), Color.White);
            Vec3<byte> blockIndex = Chunk.WorldToBlockCoords(player.Position);
            spriteBatch.DrawString(font14, $"BX: {blockIndex.X}, BY: {blockIndex.Y}, BZ: {blockIndex.Z}", 
                new Vector2(debugX, debugY + 5 * lineSpacing), Color.White);
        }

        #region IDisposable Implementation

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                // Dispose managed resources
                try
                {
                    // Dispose the inventory which implements IDisposable
                    inventory?.Dispose();
                    
                    // Note: We don't dispose shared resources like GraphicsDevice, SpriteBatch, 
                    // AssetServer, fonts, or game objects as they are managed by the game engine
                    
                    disposed = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing GameMenu: {ex.Message}");
                }
            }
        }

        ~GameMenu()
        {
            Dispose(false);
        }

        #endregion
    }
}
