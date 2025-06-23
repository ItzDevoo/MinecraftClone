using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DevCraft.Assets;
using DevCraft.GUI.Elements;
using DevCraft.GUI.Utilities;
using DevCraft.Configuration;
using DevCraft.Persistence;
using DevCraft.Utilities;


namespace DevCraft.GUI.Components
{
    class SaveGrid
    {
        public Save SelectedSave
        {
            get
            {
                if (saves == null || saves.Count == 0 || index < 0 || index >= saves.Count)
                    return null;
                return saves[index];
            }
        }

        SpriteBatch spriteBatch;

        List<Save> saves;

        SaveSlot saveSlot;
        Label pageLabel;
        Button nextPage, previousPage;

        Rectangle shading;
        Texture2D shadingTexture;

        int index;
        int page;
        Point screenSize;


        public SaveGrid(GraphicsDevice graphics, SpriteBatch spriteBatch, AssetServer assetServer, int screenWidth, int elementWidth,
            int elementHeight, List<Save> saves)
        {
            this.spriteBatch = spriteBatch;
            this.saves = saves;
            this.screenSize = new Point(screenWidth, graphics.Viewport.Height);

            SpriteFont font = assetServer.GetFont(0);
            
            // Calculate responsive dimensions
            int saveSlotWidth = (int)(screenSize.X * UIConstants.SaveGrid.SaveSlotWidthRatio);
            int saveSlotHeight = (int)(screenSize.Y * UIConstants.SaveGrid.SaveSlotHeightRatio);
            int saveSlotX = UILayoutHelper.CenterHorizontally(screenSize.X, saveSlotWidth);
            
            saveSlot = new SaveSlot(spriteBatch, saveSlotX, saveSlotWidth, saveSlotHeight,
                assetServer.GetMenuTexture("button_selector"), font, screenSize);

            // Calculate responsive page label position
            Vector2 pageLabelPosition = new Vector2(
                screenSize.X * UIConstants.SaveGrid.PageLabelLeftRatio,
                screenSize.Y * UIConstants.SaveGrid.PageControlTopRatio - (int)(screenSize.Y * 0.03f));
            pageLabel = new Label(spriteBatch, "Page ", font, pageLabelPosition, Color.White);

            // Calculate responsive button dimensions and positions
            Point buttonSize = UILayoutHelper.GetButtonSize(screenSize);
            int buttonSpacing = (int)(screenSize.X * UIConstants.SaveGrid.ButtonSpacingRatio);
            int buttonY = (int)(screenSize.Y * UIConstants.SaveGrid.PageControlTopRatio);
            
            int nextButtonX = UILayoutHelper.CenterHorizontally(screenSize.X, buttonSize.X) + buttonSpacing;
            int prevButtonX = UILayoutHelper.CenterHorizontally(screenSize.X, buttonSize.X) - buttonSize.X - buttonSpacing;

            nextPage = new Button(graphics, spriteBatch, "Next", font,
                nextButtonX, buttonY, buttonSize.X, buttonSize.Y,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    page++;
                    index = 3 * page;
                });

            previousPage = new Button(graphics, spriteBatch, "Previous", font,
                prevButtonX, buttonY, buttonSize.X, buttonSize.Y,
                assetServer.GetMenuTexture("button"), assetServer.GetMenuTexture("button_selector"), () =>
                {
                    page--;
                    index = 3 * page;
                });

            // Calculate responsive shading area
            int shadingY = (int)(screenSize.Y * UIConstants.SaveGrid.SaveGridTopRatio);
            int shadingHeight = (int)(screenSize.Y * UIConstants.SaveGrid.SaveGridHeightRatio);
            shading = new Rectangle(0, shadingY, screenSize.X, shadingHeight);
            shadingTexture = new Texture2D(graphics, screenSize.X, shadingHeight);

            Color[] darkBackGroundColor = new Color[screenSize.X * shadingHeight];
            for (int i = 0; i < darkBackGroundColor.Length; i++)
                darkBackGroundColor[i] = new Color(Color.Black, UIConstants.General.ScreenShadingAlpha);

            shadingTexture.SetData(darkBackGroundColor);

            index = 0;
            page = 0;
        }

        public void Draw()
        {
            spriteBatch.Draw(shadingTexture, shading, Color.White);

            if (saves != null && saves.Count > 0)
            {
                for (int i = 3 * page, j = 0; i < 3 * (page + 1); i++, j++)
                {
                    if (i >= saves.Count)
                    {
                        break;
                    }

                    saveSlot.DrawAt(j, saves[i], i == index);
                }
            }

            if (saves != null && saves.Count > 0)
            {
                pageLabel.Draw((page + 1).ToString());
            }
            else
            {
                pageLabel.Draw("0");
            }

            if (saves != null && page >= saves.Count / 3f - 1)
            {
                nextPage.Inactive = true;
            }
            nextPage.Draw();

            if (page == 0)
            {
                previousPage.Inactive = true;
            }
            previousPage.Draw();
        }

        public void Update(MouseState currentMouseState, MouseState previousMouseState, Point mouseLoc)
        {
            bool leftClick = Util.LeftButtonClicked(currentMouseState, previousMouseState);

            if (saves != null && index == saves.Count && index > 0)
            {
                index--;
            }

            page = index / 3;

            nextPage.Update(mouseLoc, leftClick);
            previousPage.Update(mouseLoc, leftClick);

            nextPage.Inactive = false;
            previousPage.Inactive = false;

            for (int i = 3 * page, j = 0; i < 3 * (page + 1); i++, j++)
            {
                if (saves == null || i >= saves.Count || saves.Count == 0)
                {
                    break;
                }

                if (saveSlot.ContainsAt(j, mouseLoc) && leftClick)
                {
                    index = i;
                    break;
                }
            }
        }
    }
}
