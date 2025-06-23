using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using DevCraft.Utilities;
using DevCraft.Assets;
using DevCraft.GUI.Elements;
using DevCraft.GUI.Components;
using DevCraft.GUI.Utilities;
using DevCraft.Configuration;
using DevCraft.Persistence;

namespace DevCraft.GUI.Menus
{
    class MainMenu : IDisposable
    {
        public Save CurrentSave { get; private set; }

        readonly MainGame game;
        GraphicsDevice graphics;
        SpriteBatch spriteBatch;

        Parameters newWorldParameters;

        readonly AssetServer assetServer;

        Rectangle background;
        Texture2D backgroundTexture;

        Rectangle logo;
        Texture2D logoTexture;

        Dictionary<string, GUIElement>
        mainLayout,
        newWorldLayout,
        loadWorldLayout,
        settingsLayout,
        graphicsLayout,
        displayLayout,
        texturePacksLayout;

        TextBox worldName;
        TextBox seedInput;

        Label saving, loading;

        SpriteFont font14, font24;

        MenuState state;

        enum MenuState
        {
            Main,
            Settings,
            Graphics,
            Display,
            TexturePacks,
            LoadWorld,
            NewWorld
        }

        KeyboardState previousKeyboardState;
        MouseState previousMouseState;

        int screenWidth;
        int screenHeight;

        readonly List<Save> saves;
        SaveGrid saveGrid;

        // Temporary settings for pending changes (before Apply is clicked)
        int tempRenderDistance;
        string tempDisplayMode;
        int tempMonitorIndex;


        public MainMenu(MainGame game, int screenWidth, int screenHeight, GraphicsDevice graphics, AssetServer assetServer)
        {
            this.game = game;
            game.IsMouseVisible = true;
            this.graphics = graphics;
            spriteBatch = new SpriteBatch(graphics);
            this.assetServer = assetServer;

            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;

            font14 = assetServer.GetFont(0);
            font24 = assetServer.GetFont(1);

            state = MenuState.Main;

            previousKeyboardState = Keyboard.GetState();
            previousMouseState = Mouse.GetState();

            saves = Save.LoadAll(graphics);

            InitializeGUI();
        }

        public void Update()
        {
            MouseState currentMouseState = Mouse.GetState();
            KeyboardState currentKeyboardState = Keyboard.GetState();
            Point mouseLoc = new(currentMouseState.X, currentMouseState.Y);

            switch (state)
            {
                case MenuState.Main:
                    {
                        MainControl(currentMouseState, mouseLoc);
                        break;
                    }

                case MenuState.Settings:
                    {
                        SettingsControl(currentMouseState, mouseLoc);
                        break;
                    }

                case MenuState.Graphics:
                    {
                        GraphicsControl(currentMouseState, mouseLoc);
                        break;
                    }

                case MenuState.Display:
                    {
                        DisplayControl(currentMouseState, mouseLoc);
                        break;
                    }

                case MenuState.TexturePacks:
                    {
                        TexturePacksControl(currentMouseState, mouseLoc);
                        break;
                    }

                case MenuState.LoadWorld:
                    {
                        LoadWorldControl(currentKeyboardState, currentMouseState, mouseLoc);
                        break;
                    }

                case MenuState.NewWorld:
                    {
                        NewWorldControl(currentKeyboardState, currentMouseState, mouseLoc);
                        break;
                    }
            }

            previousKeyboardState = currentKeyboardState;
            previousMouseState = currentMouseState;
        }

        public void Draw()
        {
            // Ensure proper graphics state for UI rendering
            graphics.BlendState = BlendState.AlphaBlend;
            graphics.DepthStencilState = DepthStencilState.None;
            graphics.RasterizerState = RasterizerState.CullNone;
            graphics.SamplerStates[0] = SamplerState.LinearClamp;
            
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            spriteBatch.Draw(backgroundTexture, background, Color.White);

            switch (state)
            {
                case MenuState.Main:
                    {
                        spriteBatch.Draw(logoTexture, logo, Color.White);

                        foreach (var element in mainLayout.Values)
                        {
                            element.Draw();
                        }
                        break;
                    }

                case MenuState.Settings:
                    {
                        foreach (var element in settingsLayout.Values)
                        {
                            element.Draw();
                        }
                        break;
                    }

                case MenuState.Graphics:
                    {
                        foreach (var element in graphicsLayout.Values)
                        {
                            element.Draw();
                        }

                        graphicsLayout["Render Distance"].Draw(tempRenderDistance.ToString());
                        break;
                    }

                case MenuState.Display:
                    {
                        foreach (var element in displayLayout.Values)
                        {
                            element.Draw();
                        }

                        displayLayout["Display Mode"].Draw(tempDisplayMode);
                        displayLayout["Monitor"].Draw(MainGame.GetMonitorName(tempMonitorIndex));
                        break;
                    }

                case MenuState.TexturePacks:
                    {
                        foreach (var element in texturePacksLayout.Values)
                        {
                            element.Draw();
                        }
                        break;
                    }

                case MenuState.NewWorld:
                    {
                        worldName.Draw();
                        seedInput.Draw();

                        foreach (var element in newWorldLayout.Values)
                        {
                            element.Draw();
                        }

                        newWorldLayout["World Type"].Draw(newWorldParameters.WorldType);
                        
                        // Update world type description based on current selection
                        string typeDescription = newWorldParameters.WorldType switch
                        {
                            "Default" => "Normal terrain with hills, caves, and biomes",
                            "Flat" => "Flat world, perfect for building",
                            _ => "Normal terrain with hills, caves, and biomes"
                        };
                        newWorldLayout["World Type Description"].Draw(typeDescription);
                        
                        // Show seed preview
                        string seedText = seedInput.ToString().Trim();
                        string previewText = "";
                        if (string.IsNullOrEmpty(seedText))
                        {
                            previewText = $"Generated seed: {newWorldParameters.Seed}";
                        }
                        else
                        {
                            if (int.TryParse(seedText, out int customSeed))
                            {
                                previewText = $"Using seed: {customSeed}";
                            }
                            else
                            {
                                previewText = $"Using seed: {seedText.GetHashCode()} (from \"{seedText}\")";
                            }
                        }
                        newWorldLayout["Seed Preview"].Draw(previewText);
                        
                        // Show validation message
                        string worldNameText = worldName.ToString().Trim();
                        bool nameEmpty = string.IsNullOrEmpty(worldNameText);
                        bool nameExists = saves.Exists(x => x.Name == worldNameText);
                        
                        string validationMessage = "";
                        if (nameEmpty)
                        {
                            validationMessage = "Please enter a world name";
                        }
                        else if (nameExists)
                        {
                            validationMessage = "A world with this name already exists";
                        }
                        
                        if (!string.IsNullOrEmpty(validationMessage))
                        {
                            Vector2 messageSize = font14.MeasureString(validationMessage);
                            Vector2 centeredPosition = new Vector2(screenWidth / 2 - messageSize.X / 2, screenHeight - 80);
                            spriteBatch.DrawString(font14, validationMessage, centeredPosition, Color.Red);
                        }
                        
                        break;
                    }

                case MenuState.LoadWorld:
                    {
                        saveGrid.Draw();

                        foreach (var element in loadWorldLayout.Values)
                        {
                            element.Draw();
                        }
                        
                        // Show world information when a world is selected
                        if (saveGrid.SelectedSave != null)
                        {
                            var selectedSave = saveGrid.SelectedSave;
                            string worldInfo = $"World: {selectedSave.Name} | Created: {selectedSave.Parameters.Date:MM/dd/yyyy} | " +
                                             $"Type: {selectedSave.Parameters.WorldType} | Seed: {selectedSave.Parameters.Seed}";
                            loadWorldLayout["World Info"].Draw(worldInfo);
                        }
                        else if (saves.Count == 0)
                        {
                            string noWorldsMessage = "No worlds found. Click 'Cancel' and create a new world to get started!";
                            Vector2 messageSize = font14.MeasureString(noWorldsMessage);
                            Vector2 centeredPosition = new Vector2(screenWidth / 2 - messageSize.X / 2, screenHeight / 2 - 50);
                            
                            // Draw centered message
                            spriteBatch.DrawString(font14, noWorldsMessage, centeredPosition, Color.Orange);
                        }
                        else
                        {
                            loadWorldLayout["World Info"].Draw("Select a world to view details");
                        }
                        
                        break;
                    }
            }

            spriteBatch.End();
        }

        public void DrawLoadingScreen()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            spriteBatch.Draw(backgroundTexture, background, Color.White);
            loading.Draw();

            spriteBatch.End();
        }

        public void DrawSavingScreen()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            spriteBatch.Draw(backgroundTexture, background, Color.White);
            saving.Draw();

            spriteBatch.End();
        }

        void MainControl(MouseState currentMouseState, Point mouseLoc)
        {
            bool leftClick = Util.LeftButtonClicked(currentMouseState, previousMouseState);

            foreach (var element in mainLayout.Values)
            {
                element.Update(mouseLoc, leftClick);
            }
        }

        void SettingsControl(MouseState currentMouseState, Point mouseLoc)
        {
            bool leftClick = Util.LeftButtonClicked(currentMouseState, previousMouseState);

            foreach (var element in settingsLayout.Values
            )
            {
                element.Update(mouseLoc, leftClick);
            }
        }

        void GraphicsControl(MouseState currentMouseState, Point mouseLoc)
        {
            bool leftClick = Util.LeftButtonClicked(currentMouseState, previousMouseState);
            bool rightClick = Util.RightButtonClicked(currentMouseState, previousMouseState);

            foreach (var element in graphicsLayout.Values)
            {
                element.Update(mouseLoc, leftClick);
            }

            if (graphicsLayout["Render Distance"].Clicked(mouseLoc, leftClick))
            {
                if (tempRenderDistance < 16)
                    tempRenderDistance++;
                else
                    tempRenderDistance = 1;
            }
            else if (graphicsLayout["Render Distance"].Clicked(mouseLoc, rightClick))
            {
                if (tempRenderDistance > 1)
                    tempRenderDistance--;
                else
                    tempRenderDistance = 16;
            }
        }

        void DisplayControl(MouseState currentMouseState, Point mouseLoc)
        {
            bool leftClick = Util.LeftButtonClicked(currentMouseState, previousMouseState);
            bool rightClick = Util.RightButtonClicked(currentMouseState, previousMouseState);

            foreach (var element in displayLayout.Values)
            {
                element.Update(mouseLoc, leftClick);
            }

            if (displayLayout["Display Mode"].Clicked(mouseLoc, leftClick))
            {
                switch (tempDisplayMode)
                {
                    case "Windowed":
                        tempDisplayMode = "Borderless";
                        break;
                    case "Borderless":
                        tempDisplayMode = "Fullscreen";
                        break;
                    case "Fullscreen":
                        tempDisplayMode = "Windowed";
                        break;
                }
            }
            else if (displayLayout["Monitor"].Clicked(mouseLoc, leftClick))
            {
                tempMonitorIndex++;
                if (tempMonitorIndex >= MainGame.GetMonitorCount())
                    tempMonitorIndex = 0;
            }
            else if (displayLayout["Monitor"].Clicked(mouseLoc, rightClick))
            {
                tempMonitorIndex--;
                if (tempMonitorIndex < 0)
                    tempMonitorIndex = MainGame.GetMonitorCount() - 1;
            }
        }

        void NewWorldControl(KeyboardState currentKeyboardState, MouseState currentMouseState, Point mouseLoc)
        {
            bool leftClick = Util.LeftButtonClicked(currentMouseState, previousMouseState);
            bool rightClick = Util.RightButtonClicked(currentMouseState, previousMouseState);

            string worldNameText = worldName.ToString().Trim();
            
            // Disable create button if world name is empty or already exists
            bool nameEmpty = string.IsNullOrEmpty(worldNameText);
            bool nameExists = saves.Exists(x => x.Name == worldNameText);
            
            newWorldLayout["Create World"].Inactive = nameEmpty || nameExists;

            worldName.Update(mouseLoc, currentKeyboardState, previousKeyboardState, leftClick, rightClick);
            seedInput.Update(mouseLoc, currentKeyboardState, previousKeyboardState, leftClick, rightClick);

            foreach (var element in newWorldLayout.Values)
            {
                element.Update(mouseLoc, leftClick);
            }
        }

        void LoadWorldControl(KeyboardState currentKeyboardState, MouseState currentMouseState, Point mouseLoc)
        {
            bool leftClick = Util.LeftButtonClicked(currentMouseState, previousMouseState);

            bool hasWorlds = saves.Count > 0;
            bool hasSelection = hasWorlds && saveGrid.SelectedSave != null;

            loadWorldLayout["Play"].Inactive = !hasSelection;
            loadWorldLayout["Edit"].Inactive = !hasSelection;
            loadWorldLayout["Delete"].Inactive = !hasSelection;

            foreach (var element in loadWorldLayout.Values)
            {
                element.Update(mouseLoc, leftClick);
            }

            saveGrid.Update(currentMouseState, previousMouseState, mouseLoc);
        }

        void TexturePacksControl(MouseState currentMouseState, Point mouseLoc)
        {
            bool leftClick = Util.LeftButtonClicked(currentMouseState, previousMouseState);

            foreach (var element in texturePacksLayout.Values)
            {
                element.Update(mouseLoc, leftClick);
            }
        }

        void InitializeGUI()
        {
            // Get screen size for responsive calculations
            Point screenSize = new Point(screenWidth, screenHeight);
            
            // Use responsive button sizing
            Point buttonSize = UILayoutHelper.GetButtonSize(screenSize);
            int elementWidth = buttonSize.X;
            int elementHeight = buttonSize.Y;

            // Use responsive background scaling
            backgroundTexture = assetServer.GetMenuTexture("menu_background");
            background = UILayoutHelper.GetScaledBackgroundBounds(screenSize, 
                new Point(backgroundTexture.Width, backgroundTexture.Height));

            // Use responsive logo scaling
            logoTexture = assetServer.GetMenuTexture("logo");
            logo = UILayoutHelper.GetLogoBounds(screenSize, 
                new Point(logoTexture.Width, logoTexture.Height));

            // Center loading text
            Vector2 savingTextSize = font24.MeasureString("Saving World");
            saving = new Label(spriteBatch, "Saving World", font24,
                new Vector2(UILayoutHelper.CenterHorizontally(screenWidth, (int)savingTextSize.X), 
                           UILayoutHelper.CenterVertically(screenHeight, (int)savingTextSize.Y)), Color.White);

            Vector2 loadingTextSize = font24.MeasureString("Loading World");
            loading = new Label(spriteBatch, "Loading World", font24,
                new Vector2(UILayoutHelper.CenterHorizontally(screenWidth, (int)loadingTextSize.X),
                           UILayoutHelper.CenterVertically(screenHeight, (int)loadingTextSize.Y)), Color.White);

            // Calculate button positions using responsive layout  
            int buttonSpacing = UILayoutHelper.GetButtonSpacing(screenSize);
            int buttonStartY = UILayoutHelper.GetMenuButtonTopMargin(screenSize);

            mainLayout = new Dictionary<string, GUIElement>()
            {
                ["New World"] = new Button(graphics, spriteBatch, "New World", font14,
                UILayoutHelper.CenterHorizontally(screenWidth, elementWidth), 
                buttonStartY, 
                elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    newWorldParameters = new Parameters();
                    worldName.Clear();
                    seedInput.Clear();
                    state = MenuState.NewWorld;
                }),

                ["Load World"] = new Button(graphics, spriteBatch, "Load World", font14,
                UILayoutHelper.CenterHorizontally(screenWidth, elementWidth), 
                buttonStartY + buttonSpacing, 
                elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    saves.Sort((a, b) => a.Parameters.Date.CompareTo(b.Parameters.Date));
                    saves.Reverse();

                    state = MenuState.LoadWorld;
                }),

                ["Settings"] = new Button(graphics, spriteBatch, "Settings", font14,
                UILayoutHelper.CenterHorizontally(screenWidth, elementWidth), 
                buttonStartY + buttonSpacing * 2, 
                elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    state = MenuState.Settings;
                }),

                ["Quit"] = new Button(graphics, spriteBatch, "Quit Game", font14,
                UILayoutHelper.CenterHorizontally(screenWidth, elementWidth), 
                buttonStartY + buttonSpacing * 3, 
                elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    game.Exit();
                })
            };


            settingsLayout = new Dictionary<string, GUIElement>()
            {
                ["Settings Title"] = new Label(spriteBatch, "Settings", font24,
                new Vector2(screenWidth / 2 - font24.MeasureString("Settings").X / 2, 40), Color.White),

                ["Graphics"] = new Button(graphics, spriteBatch, "Graphics", font14,
                UILayoutHelper.CenterHorizontally(screenWidth, elementWidth), 
                UILayoutHelper.CenterVertically(screenHeight, elementHeight) - buttonSpacing, 
                elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    LoadTempSettings();
                    state = MenuState.Graphics;
                }),

                ["Texture Packs"] = new Button(graphics, spriteBatch, "Texture Packs", font14,
                UILayoutHelper.CenterHorizontally(screenWidth, elementWidth), 
                UILayoutHelper.CenterVertically(screenHeight, elementHeight),
                elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    state = MenuState.TexturePacks;
                }),

                ["Display"] = new Button(graphics, spriteBatch, "Display", font14,
                UILayoutHelper.CenterHorizontally(screenWidth, elementWidth), 
                UILayoutHelper.CenterVertically(screenHeight, elementHeight) + buttonSpacing,
                elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    LoadTempSettings();
                    state = MenuState.Display;
                }),

                ["Back"] = new Button(graphics, spriteBatch, "Back", font14,
                screenWidth - elementWidth - UILayoutHelper.GetDefaultMargin(screenSize), 
                screenHeight - elementHeight - UILayoutHelper.GetDefaultMargin(screenSize), 
                elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    state = MenuState.Main;
                })
            };


            newWorldLayout = new Dictionary<string, GUIElement>()
            {
                ["Create World Label"] = new Label(spriteBatch, "Create New World", font24,
                new Vector2(UILayoutHelper.CenterHorizontally(screenWidth, (int)font24.MeasureString("Create New World").X), 
                           (int)(screenHeight * UIConstants.Buttons.LogoTopMarginRatio)), Color.White),

                ["World Name Label"] = new Label(spriteBatch, "World Name", font14,
                new Vector2(UILayoutHelper.CenterHorizontally(screenWidth, (int)font14.MeasureString("World Name").X), 
                           (int)(screenHeight * 0.25f)), Color.White),

                ["Seed Label"] = new Label(spriteBatch, "Seed (leave empty for random)", font14,
                new Vector2(UILayoutHelper.CenterHorizontally(screenWidth, (int)font14.MeasureString("Seed (leave empty for random)").X), 
                           (int)(screenHeight * 0.40f)), Color.White),

                ["Seed Preview"] = new Label(spriteBatch, "", font14,
                new Vector2(UILayoutHelper.CenterHorizontally(screenWidth, 200), 
                           (int)(screenHeight * 0.50f)), Color.LightGray),

                ["World Type Label"] = new Label(spriteBatch, "World Type", font14,
                new Vector2(UILayoutHelper.CenterHorizontally(screenWidth, (int)font14.MeasureString("World Type").X), 
                           (int)(screenHeight * 0.60f)), Color.White),

                ["World Type"] = new Button(graphics, spriteBatch, "Default", font14,
                UILayoutHelper.CenterHorizontally(screenWidth, elementWidth), 
                (int)(screenHeight * 0.65f), elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    if (newWorldParameters.WorldType == "Default")
                    {
                        newWorldParameters.WorldType = "Flat";
                    }
                    else
                    {
                        newWorldParameters.WorldType = "Default";
                    }
                }),

                // Improved description positioning and shorter text to prevent wrapping
                ["World Type Description"] = new Label(spriteBatch, "Normal terrain with hills, caves, and biomes", font14,
                new Vector2(UILayoutHelper.CenterHorizontally(screenWidth, (int)font14.MeasureString("Normal terrain with hills, caves, and biomes").X), 
                           (int)(screenHeight * 0.75f)), Color.LightGray),

                ["Validation Message"] = new Label(spriteBatch, "", font14,
                new Vector2(UILayoutHelper.CenterHorizontally(screenWidth, 300), 
                           (int)(screenHeight * 0.82f)), Color.Red),

                ["Create World"] = new Button(graphics, spriteBatch, "Create World", font14,
                UILayoutHelper.CenterHorizontally(screenWidth, elementWidth * 2 + buttonSpacing) - elementWidth - buttonSpacing/2, 
                screenHeight - elementHeight - UILayoutHelper.GetDefaultMargin(screenSize), elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    string saveName = worldName.ToString();

                    // Handle seed input - use provided seed or generate random one
                    string seedText = seedInput.ToString().Trim();
                    if (!string.IsNullOrEmpty(seedText))
                    {
                        if (int.TryParse(seedText, out int customSeed))
                        {
                            newWorldParameters.Seed = customSeed;
                        }
                        else
                        {
                            // If seed is not a valid number, use the string's hash code
                            newWorldParameters.Seed = seedText.GetHashCode();
                        }
                    }
                    // If empty, Parameters constructor already generated a random seed

                    CurrentSave = new Save(saveName, newWorldParameters);

                    saves.Add(CurrentSave);

                    Directory.CreateDirectory($"Saves/{saveName}");

                    state = MenuState.Main;
                    game.IsMouseVisible = false;
                    game.State = GameState.Loading;
                }),

                ["Cancel"] = new Button(graphics, spriteBatch, "Cancel", font14,
                UILayoutHelper.CenterHorizontally(screenWidth, elementWidth * 2 + buttonSpacing) + elementWidth + buttonSpacing/2, 
                screenHeight - elementHeight - UILayoutHelper.GetDefaultMargin(screenSize), elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    state = MenuState.Main;
                })
            };

            worldName = new TextBox(game.Window, graphics, spriteBatch,
                UILayoutHelper.CenterHorizontally(screenWidth, elementWidth), 
                (int)(screenHeight * 0.30f), elementWidth, elementHeight,
                assetServer.GetMenuTexture("button_selector"), font14);

            seedInput = new TextBox(game.Window, graphics, spriteBatch,
                UILayoutHelper.CenterHorizontally(screenWidth, elementWidth), 
                (int)(screenHeight * 0.45f), elementWidth, elementHeight,
                assetServer.GetMenuTexture("button_selector"), font14);


            loadWorldLayout = new Dictionary<string, GUIElement>()
            {
                ["Select World"] = new Label(spriteBatch, "Select World", font24,
                new Vector2(UILayoutHelper.CenterHorizontally(screenWidth, (int)font24.MeasureString("Select World").X), 
                           (int)(screenHeight * UIConstants.Buttons.LogoTopMarginRatio)), Color.White),

                ["World Info"] = new Label(spriteBatch, "Select a world to view details", font14,
                new Vector2(UILayoutHelper.CenterHorizontally(screenWidth, (int)font14.MeasureString("Select a world to view details").X), 
                           screenHeight - (int)(screenHeight * 0.15f)), Color.LightGray),

                ["No Worlds Message"] = new Label(spriteBatch, "", font14,
                new Vector2(UILayoutHelper.CenterHorizontally(screenWidth, 200), 
                           UILayoutHelper.CenterVertically(screenHeight, (int)font14.LineSpacing)), Color.Orange),

                ["Play"] = new Button(graphics, spriteBatch, "Play Selected World", font14,
                UILayoutHelper.CenterHorizontally(screenWidth, elementWidth * 3 + 40) - elementWidth - 20, 
                screenHeight - elementHeight - 30, elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    CurrentSave = saveGrid.SelectedSave;
                    state = MenuState.Main;
                    game.IsMouseVisible = false;
                    game.State = GameState.Loading;
                }),

                ["Edit"] = new Button(graphics, spriteBatch, "Edit", font14,
                UILayoutHelper.CenterHorizontally(screenWidth, elementWidth * 3 + 40), 
                screenHeight - elementHeight - 30, elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    // TODO: Implement world editing (rename, backup, etc.)
                }),

                ["Delete"] = new Button(graphics, spriteBatch, "Delete", font14,
                UILayoutHelper.CenterHorizontally(screenWidth, elementWidth * 3 + 40) + elementWidth + 20, 
                screenHeight - elementHeight - 30, elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    Save toDelete = saveGrid.SelectedSave;
                    if (toDelete != null)
                    {
                        Directory.Delete($"Saves/{toDelete.Name}", true);
                        saves.Remove(toDelete);
                    }
                }),

                ["Cancel"] = new Button(graphics, spriteBatch, "Cancel", font14,
                50, screenHeight - elementHeight - 30, elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    state = MenuState.Main;
                })
            };

            saveGrid = new SaveGrid(graphics, spriteBatch, assetServer, screenWidth,
                elementWidth, elementHeight, saves);

            graphicsLayout = new Dictionary<string, GUIElement>()
            {
                ["Graphics Settings"] = new Label(spriteBatch, "Graphics Settings", font24,
                new Vector2(screenWidth / 2 - font24.MeasureString("Graphics Settings").X / 2, 40), Color.White),

                ["Render Distance"] = new Button(graphics, spriteBatch, "Render Distance: ", font14,
                screenWidth / 2 - elementWidth / 2, screenHeight / 2 - 60, elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector")),

                ["Apply"] = new Button(graphics, spriteBatch, "Apply", font14,
                0, screenHeight - elementHeight, elementWidth / 2, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    ApplyTempSettings();
                }),

                ["Cancel"] = new Button(graphics, spriteBatch, "Cancel", font14,
                screenWidth - elementWidth / 2, screenHeight - elementHeight, elementWidth / 2, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    state = MenuState.Settings;
                })
            };

            displayLayout = new Dictionary<string, GUIElement>()
            {
                ["Display Settings"] = new Label(spriteBatch, "Display Settings", font24,
                new Vector2(screenWidth / 2 - font24.MeasureString("Display Settings").X / 2, 40), Color.White),

                ["Display Mode"] = new Button(graphics, spriteBatch, "Display Mode: ", font14,
                screenWidth / 2 - elementWidth / 2, screenHeight / 2 - 80, elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector")),

                ["Monitor"] = new Button(graphics, spriteBatch, "Monitor: ", font14,
                screenWidth / 2 - elementWidth / 2, screenHeight / 2 - 20, elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector")),

                ["Apply"] = new Button(graphics, spriteBatch, "Apply", font14,
                0, screenHeight - elementHeight, elementWidth / 2, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    ApplyTempSettings();
                }),

                ["Cancel"] = new Button(graphics, spriteBatch, "Cancel", font14,
                screenWidth - elementWidth / 2, screenHeight - elementHeight, elementWidth / 2, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    state = MenuState.Settings;
                })
            };

            texturePacksLayout = new Dictionary<string, GUIElement>()
            {
                ["Texture Packs Title"] = new Label(spriteBatch, "Texture Packs", font24,
                new Vector2(UILayoutHelper.CenterHorizontally(screenWidth, (int)font24.MeasureString("Texture Packs").X), 40), Color.White),

                ["Current Pack"] = new Label(spriteBatch, "Current: Default", font14,
                new Vector2(UILayoutHelper.CenterHorizontally(screenWidth, (int)font14.MeasureString("Current: Default").X), 100), Color.LightGray),

                ["Instructions"] = new Label(spriteBatch, "Place texture packs in Assets/TexturePacks/", font14,
                new Vector2(UILayoutHelper.CenterHorizontally(screenWidth, (int)font14.MeasureString("Place texture packs in Assets/TexturePacks/").X), 150), Color.Yellow),

                ["Instructions2"] = new Label(spriteBatch, "Support for Minecraft resource packs coming soon!", font14,
                new Vector2(UILayoutHelper.CenterHorizontally(screenWidth, (int)font14.MeasureString("Support for Minecraft resource packs coming soon!").X), 180), Color.Orange),

                ["Back"] = new Button(graphics, spriteBatch, "Back", font14,
                50, screenHeight - elementHeight - 30, elementWidth, elementHeight,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    state = MenuState.Settings;
                })
            };
        }

        public void RefreshLayout(int newScreenWidth, int newScreenHeight)
        {
            this.screenWidth = newScreenWidth;
            this.screenHeight = newScreenHeight;
            
            // Reinitialize all GUI elements with new screen dimensions
            InitializeGUI();
        }

        void LoadTempSettings()
        {
            tempRenderDistance = Settings.RenderDistance;
            tempDisplayMode = Settings.DisplayMode;
            tempMonitorIndex = Settings.MonitorIndex;
        }

        void ApplyTempSettings()
        {
            Settings.RenderDistance = tempRenderDistance;
            Settings.DisplayMode = tempDisplayMode;
            Settings.MonitorIndex = tempMonitorIndex;
            game.ApplyDisplayMode();
            Settings.Save();
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
                    // Dispose UI components that implement IDisposable
                    if (saveGrid is IDisposable disposableSaveGrid)
                    {
                        disposableSaveGrid.Dispose();
                    }

                    // Note: We don't dispose shared resources like GraphicsDevice, SpriteBatch, 
                    // AssetServer, fonts, or textures as they are managed by the game engine
                    
                    disposed = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing MainMenu: {ex.Message}");
                }
            }
        }

        ~MainMenu()
        {
            Dispose(false);
        }

        #endregion
    }
}
