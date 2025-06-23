using System;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using DevCraft.Utilities;
using DevCraft.Assets;
using DevCraft.World.Blocks;
using DevCraft.GUI.Elements;
using DevCraft.GUI.Utilities;
using DevCraft.Configuration;
using DevCraft.Persistence;


namespace DevCraft.GUI.Components
{
    class Inventory : IDisposable
    {
        public ushort SelectedItem { get; private set; }

        MainGame game;
        SpriteBatch spriteBatch;
        readonly AssetServer assetServer;
        readonly BlockMetadataProvider blockMetadata;
        readonly UIResourceManager resourceManager;

        Action action;

        Label inventory, hotbar;
        Scroller scroller;

        Texture2D blackTexture;
        SpriteFont font;

        int previousScrollValue;
        int activeItemIndex;

        int screenWidth;
        int screenHeight;

        ushort[,] items;

        ushort[] hotbarItems;
        ushort draggedTexture;
        ushort hoveredTexture;
        Rectangle selector, selectedItem;

        // Cache layout calculations
        Rectangle hotbarBounds;
        Rectangle inventoryBounds;


        public Inventory(MainGame game, SpriteBatch spriteBatch, SpriteFont font,
            Parameters parameters, AssetServer assetServer, BlockMetadataProvider blockMetadata, Action a = null)
        {
            this.game = game;
            this.spriteBatch = spriteBatch;
            this.font = font;
            this.assetServer = assetServer;
            this.blockMetadata = blockMetadata;
            this.resourceManager = new UIResourceManager(game.GraphicsDevice);
            action = a;

            screenWidth = game.Window.ClientBounds.Width;
            screenHeight = game.Window.ClientBounds.Height;

            // Calculate responsive layout bounds
            var screenSize = new Point(screenWidth, screenHeight);
            hotbarBounds = UILayoutHelper.GetHotbarBounds(screenSize);
            inventoryBounds = UILayoutHelper.GetInventoryBounds(screenSize);

            activeItemIndex = 0;

            hotbarItems = new ushort[UIConstants.Inventory.HotbarSlots];
            if (parameters.Inventory.Any(item => item != Block.EmptyValue))
            {
                hotbarItems = parameters.Inventory;
            }

            int nRows = blockMetadata.BlockCount / UIConstants.Inventory.InventoryColumns + 1;
            items = new ushort[nRows, UIConstants.Inventory.InventoryColumns];

            int col = 0, row = 0;
            foreach (ushort blockIndex in blockMetadata.GetBlockIds)
            {
                items[row, col] = blockIndex;
                col++;

                if (col % UIConstants.Inventory.InventoryColumns == 0)
                {
                    col = 0;
                    row++;
                }
            }

            draggedTexture = Block.EmptyValue;
            hoveredTexture = Block.EmptyValue;

            SelectedItem = hotbarItems[activeItemIndex];

            previousScrollValue = 0;

            // Use responsive positioning
            inventory = new Label(spriteBatch, assetServer.GetMenuTexture("inventory"),
                inventoryBounds, Color.White);

            hotbar = new Label(spriteBatch, assetServer.GetMenuTexture("toolbar"),
                hotbarBounds, UIConstants.Colors.HotbarBackground);

            // Use resource manager for textures
            Texture2D scrollerTexture = resourceManager.GetScrollerTexture();

            int scrollerHeight = (int)(screenHeight * UIConstants.Inventory.ScrollerHeightRatio);
            int scrollStep = scrollerHeight / (nRows - UIConstants.Inventory.ScrollerRowBuffer) > 0 ? 
                           scrollerHeight / (nRows - UIConstants.Inventory.ScrollerRowBuffer) : 1;
                           
            scroller = new Scroller(spriteBatch, scrollerTexture, nRows, scrollStep,
                new Rectangle(screenWidth - (int)(screenWidth * UIConstants.Inventory.ScrollerRightMarginRatio), 
                             inventoryBounds.Y + (int)(screenHeight * UIConstants.Inventory.ScrollerTopOffsetRatio), 
                             (int)(screenWidth * UIConstants.Inventory.ScrollerWidthRatio), 
                             scrollerHeight / (nRows / UIConstants.Inventory.ScrollerMinRowsVisible + 1)));

            // Position selector and selected item relative to hotbar
            int slotSize = (int)(screenWidth * UIConstants.Inventory.SlotSizeRatio);
            int selectorBorder = (int)(screenWidth * UIConstants.Inventory.SelectorBorderRatio);
            int selectorInset = (int)(screenWidth * UIConstants.Inventory.SelectorInsetRatio);
            
            selector = new Rectangle(hotbarBounds.X, hotbarBounds.Y, slotSize + selectorBorder, slotSize + selectorBorder);
            selectedItem = new Rectangle(hotbarBounds.X + selectorInset, hotbarBounds.Y + selectorInset, 
                                       slotSize, slotSize);

            // Use resource manager for black texture
            blackTexture = resourceManager.GetBlackTexture();
        }

        public void SaveParameters(Parameters parameters)
        {
            parameters.Inventory = hotbarItems;
        }

        public void Draw(MouseState currentMouseState)
        {
            inventory.Draw();
            scroller.Draw();

            int startRow = scroller.Start;
            int endRow = scroller.End;

            //Draw all blocks in the inventory
            for (int row = startRow, i = 0; row < endRow; row++, i++)
            {
                for (int j = 0; j < UIConstants.Inventory.InventoryColumns; j++)
                {
                    if (items[row, j] != Block.EmptyValue)
                    {
                        var slotBounds = UILayoutHelper.GetInventorySlotBounds(inventoryBounds, row * UIConstants.Inventory.InventoryColumns + j, startRow, new Point(screenWidth, screenHeight));
                        spriteBatch.Draw(assetServer.GetBlockTexture(items[row, j]), slotBounds, Color.White);
                    }
                }
            }

            //Draw hotbar inside the inventory
            int slotSize = (int)(screenWidth * UIConstants.Inventory.SlotSizeRatio);
            int slotSpacing = (int)(screenWidth * UIConstants.Inventory.SlotSpacingRatio);
            for (int i = 0; i < UIConstants.Inventory.HotbarSlots; i++)
            {
                if (hotbarItems[i] != Block.EmptyValue)
                {
                    var hotbarSlotBounds = new Rectangle(
                        inventoryBounds.X + (int)(inventoryBounds.Width * 0.05f) + i * slotSpacing,
                        screenHeight - (int)(screenHeight * UIConstants.Inventory.HotbarBottomMarginRatio) - slotSize - (int)(screenHeight * 0.08f), 
                        slotSize, 
                        slotSize);
                    spriteBatch.Draw(assetServer.GetBlockTexture(hotbarItems[i]), hotbarSlotBounds, Color.White);
                }
            }

            //Draw the selected dragged texture
            if (draggedTexture != Block.EmptyValue)
            {
                var draggedBounds = new Rectangle(
                    currentMouseState.X, 
                    currentMouseState.Y, 
                    slotSize, 
                    slotSize);
                spriteBatch.Draw(assetServer.GetBlockTexture(draggedTexture), draggedBounds, Color.White);
            }

            //Draw the name of the hovered texture
            if (hoveredTexture != Block.EmptyValue)
            {
                Vector2 textSize = font.MeasureString(blockMetadata.GetBlockName(hoveredTexture));

                int tooltipOffsetX = (int)(screenWidth * UIConstants.Inventory.TooltipOffsetXRatio);
                int tooltipOffsetY = (int)(screenHeight * UIConstants.Inventory.TooltipOffsetYRatio);
                var tooltipBounds = new Rectangle(
                    currentMouseState.X + tooltipOffsetX, 
                    currentMouseState.Y + tooltipOffsetY,
                    (int)textSize.X, 
                    (int)textSize.Y);
                
                spriteBatch.Draw(blackTexture, tooltipBounds, UIConstants.Colors.TooltipBackground);

                spriteBatch.DrawString(font, blockMetadata.GetBlockName(hoveredTexture),
                    new Vector2(tooltipBounds.X, tooltipBounds.Y), UIConstants.Colors.TooltipText);

                hoveredTexture = Block.EmptyValue;
            }
        }

        public void DrawHotbar()
        {
            hotbar.Draw();

            for (int i = 0; i < hotbarItems.Length; i++)
            {
                var slotBounds = UILayoutHelper.GetHotbarSlotBounds(hotbarBounds, i, new Point(screenWidth, screenHeight));

                if (hotbarItems[i] != Block.EmptyValue)
                {
                    spriteBatch.Draw(assetServer.GetBlockTexture(hotbarItems[i]), slotBounds, Color.White);
                }
            }

            //Draw selected item name
            if (hotbarItems[activeItemIndex] != Block.EmptyValue)
            {
                Vector2 textSize = font.MeasureString(blockMetadata.GetBlockName(hotbarItems[activeItemIndex]));
                
                int x = hotbarBounds.X;
                int y = hotbarBounds.Y + (int)(screenHeight * UIConstants.Inventory.NameDisplayOffsetYRatio);

                spriteBatch.Draw(blackTexture,
                    new Rectangle(x, y, (int)textSize.X, (int)textSize.Y), UIConstants.Colors.TooltipBackground);

                spriteBatch.DrawString(font, blockMetadata.GetBlockName(hotbarItems[activeItemIndex]),
                    new Vector2(x, y), UIConstants.Colors.TooltipText);
            }

            spriteBatch.Draw(assetServer.GetMenuTexture("selector"), selector, Color.White);
        }

        public void Update(KeyboardState currentKeyboardState, KeyboardState previousKeyboardState,
                           MouseState currentMouseState, MouseState previousMouseState, Point mouseLoc)
        {
            // Add null check for action delegate
            if (Util.KeyPressed(Keys.Escape, currentKeyboardState, previousKeyboardState) ||
                Util.KeyPressed(Keys.E, currentKeyboardState, previousKeyboardState))
            {
                action?.Invoke();
                return;
            }

            bool itemSelected = false;
            bool leftClick = Util.LeftButtonClicked(currentMouseState, previousMouseState);
            bool rightClick = Util.RightButtonClicked(currentMouseState, previousMouseState);

            // Validate scroller state
            if (scroller == null) return;
            
            scroller.Update(currentMouseState, previousMouseState, mouseLoc);

            int startRow = Math.Max(0, scroller.Start);
            int endRow = Math.Min(items?.GetLength(0) ?? 0, scroller.End);

            // Validate hotbar items array
            if (hotbarItems == null) return;

            int slotSize = (int)(screenWidth * UIConstants.Inventory.SlotSizeRatio);
            int slotSpacing = (int)(screenWidth * UIConstants.Inventory.SlotSpacingRatio);

            for (int i = 0; i < hotbarItems.Length; i++)
            {
                var hotbarSlotBounds = new Rectangle(
                    inventoryBounds.X + (int)(inventoryBounds.Width * 0.05f) + i * slotSpacing,
                    screenHeight - (int)(screenHeight * UIConstants.Inventory.HotbarBottomMarginRatio) - slotSize - (int)(screenHeight * 0.08f), 
                    slotSize, 
                    slotSize);
                
                bool clicked = leftClick && hotbarSlotBounds.Contains(mouseLoc);

                if (hotbarItems[i] == Block.EmptyValue && draggedTexture != Block.EmptyValue && clicked)
                {
                    hotbarItems[i] = draggedTexture;
                    draggedTexture = Block.EmptyValue;
                }
                else if (draggedTexture == Block.EmptyValue && hotbarItems[i] != Block.EmptyValue && clicked)
                {
                    draggedTexture = hotbarItems[i];
                    hotbarItems[i] = Block.EmptyValue;
                    itemSelected = true;
                }
                else if (hotbarItems[i] != Block.EmptyValue && draggedTexture != Block.EmptyValue && clicked)
                {
                    (hotbarItems[i], draggedTexture) = (draggedTexture, hotbarItems[i]);
                    itemSelected = true;
                }

                if (hotbarItems[i] != Block.EmptyValue && hotbarSlotBounds.Contains(mouseLoc))
                {
                    hoveredTexture = hotbarItems[i];
                }
            }

            if (!itemSelected && items != null)
            {
                for (int row = startRow, i = 0; row < endRow; row++, i++)
                {
                    for (int j = 0; j < UIConstants.Inventory.InventoryColumns; j++)
                    {
                        // Bounds check for items array
                        if (row >= items.GetLength(0) || j >= items.GetLength(1)) 
                            continue;
                            
                        var inventorySlotBounds = UILayoutHelper.GetInventorySlotBounds(inventoryBounds, row * UIConstants.Inventory.InventoryColumns + j, startRow);
                        
                        if (items[row, j] != Block.EmptyValue &&
                            leftClick &&
                            inventorySlotBounds.Contains(mouseLoc))
                        {
                            draggedTexture = items[row, j];
                            itemSelected = true;
                        }

                        if (items[row, j] != Block.EmptyValue &&
                            inventorySlotBounds.Contains(mouseLoc))
                        {
                            hoveredTexture = items[row, j];
                        }
                    }
                }
            }

            if (rightClick || !itemSelected && leftClick)
            {
                draggedTexture = Block.EmptyValue;
            }
        }

        public void UpdateHotbar(MouseState currentMouseState)
        {
            if (currentMouseState.ScrollWheelValue - previousScrollValue < 0)
            {
                if (activeItemIndex == 8)
                {
                    activeItemIndex = 0;
                }

                else
                {
                    activeItemIndex++;
                }
            }

            else if (currentMouseState.ScrollWheelValue - previousScrollValue > 0)
            {
                if (activeItemIndex == 0)
                {
                    activeItemIndex = 8;
                }

                else
                {
                    activeItemIndex--;
                }
            }

            // Update selector position using responsive layout
            var selectorOffset = activeItemIndex * (hotbarBounds.Width / UIConstants.Inventory.HotbarSlots);
            selector.X = hotbarBounds.X + selectorOffset;

            SelectedItem = hotbarItems[activeItemIndex];

            previousScrollValue = currentMouseState.ScrollWheelValue;
        }

        public void Dispose()
        {
            resourceManager?.Dispose();
        }
    }
}
