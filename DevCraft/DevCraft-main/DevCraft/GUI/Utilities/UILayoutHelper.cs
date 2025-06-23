using System;
using Microsoft.Xna.Framework;
using DevCraft.Configuration;

namespace DevCraft.GUI.Utilities
{
    /// <summary>
    /// Provides utility methods for UI layout calculations and responsive positioning.
    /// Helps eliminate hardcoded pixel values by providing percentage-based positioning.
    /// </summary>
    public static class UILayoutHelper
    {
        /// <summary>
        /// Calculates a position based on screen percentage.
        /// </summary>
        /// <param name="screenSize">The screen dimensions</param>
        /// <param name="percentageX">X position as percentage (0.0 to 1.0)</param>
        /// <param name="percentageY">Y position as percentage (0.0 to 1.0)</param>
        /// <returns>Absolute position in pixels</returns>
        public static Point GetPercentagePosition(Point screenSize, float percentageX, float percentageY)
        {
            return new Point(
                (int)(screenSize.X * percentageX),
                (int)(screenSize.Y * percentageY)
            );
        }
        
        /// <summary>
        /// Centers an element horizontally on the screen.
        /// </summary>
        /// <param name="screenWidth">Screen width</param>
        /// <param name="elementWidth">Width of the element to center</param>
        /// <returns>X position for centering</returns>
        public static int CenterHorizontally(int screenWidth, int elementWidth)
        {
            return (screenWidth - elementWidth) / 2;
        }
        
        /// <summary>
        /// Centers an element vertically on the screen.
        /// </summary>
        /// <param name="screenHeight">Screen height</param>
        /// <param name="elementHeight">Height of the element to center</param>
        /// <returns>Y position for centering</returns>
        public static int CenterVertically(int screenHeight, int elementHeight)
        {
            return (screenHeight - elementHeight) / 2;
        }
          /// <summary>
        /// Calculates the hotbar position based on screen dimensions.
        /// </summary>
        /// <param name="screenSize">Current screen dimensions</param>
        /// <returns>Rectangle representing the hotbar area</returns>
        public static Rectangle GetHotbarBounds(Point screenSize)
        {
            int hotbarWidth = (int)(screenSize.X * UIConstants.Inventory.HotbarWidthRatio);
            int hotbarHeight = (int)(screenSize.Y * UIConstants.Inventory.HotbarHeightRatio);
            int x = CenterHorizontally(screenSize.X, hotbarWidth);
            int y = screenSize.Y - (int)(screenSize.Y * UIConstants.Inventory.HotbarBottomMarginRatio) - hotbarHeight;
            
            return new Rectangle(x, y, hotbarWidth, hotbarHeight);
        }
        
        /// <summary>
        /// Calculates the inventory panel position.
        /// </summary>
        /// <param name="screenSize">Current screen dimensions</param>
        /// <returns>Rectangle representing the inventory panel area</returns>
        public static Rectangle GetInventoryBounds(Point screenSize)
        {
            int x = (int)(screenSize.X * UIConstants.Inventory.InventoryLeftRatio);
            int y = (int)(screenSize.Y * UIConstants.Inventory.InventoryTopRatio);
            int width = (int)(screenSize.X * UIConstants.Inventory.InventoryWidthRatio);
            int height = (int)(screenSize.Y * UIConstants.Inventory.InventoryHeightRatio);
            
            return new Rectangle(x, y, width, height);
        }
          /// <summary>
        /// Calculates slot positions within the inventory grid.
        /// </summary>
        /// <param name="inventoryBounds">The inventory panel bounds</param>
        /// <param name="slotIndex">Index of the slot (0-based)</param>
        /// <param name="rowOffset">Row offset for scrolling</param>
        /// <param name="screenSize">Screen dimensions for responsive calculations</param>
        /// <returns>Rectangle representing the slot bounds</returns>
        public static Rectangle GetInventorySlotBounds(Rectangle inventoryBounds, int slotIndex, int rowOffset, Point screenSize)
        {
            int column = slotIndex % UIConstants.Inventory.InventoryColumns;
            int row = (slotIndex / UIConstants.Inventory.InventoryColumns) - rowOffset;
            
            int slotSize = (int)(screenSize.X * UIConstants.Inventory.SlotSizeRatio);
            int slotSpacing = (int)(screenSize.X * UIConstants.Inventory.SlotSpacingRatio);
            
            int x = inventoryBounds.X + (int)(inventoryBounds.Width * 0.05f) + column * slotSpacing;
            int y = inventoryBounds.Y + (int)(inventoryBounds.Height * 0.15f) + row * slotSpacing;
            
            return new Rectangle(x, y, slotSize, slotSize);
        }
        
        /// <summary>
        /// Calculates slot positions within the inventory grid (overload for backward compatibility).
        /// </summary>
        /// <param name="inventoryBounds">The inventory panel bounds</param>
        /// <param name="slotIndex">Index of the slot (0-based)</param>
        /// <param name="rowOffset">Row offset for scrolling</param>
        /// <returns>Rectangle representing the slot bounds</returns>
        public static Rectangle GetInventorySlotBounds(Rectangle inventoryBounds, int slotIndex, int rowOffset = 0)
        {
            // Use a default screen size for legacy calls - should be updated to use the new overload
            return GetInventorySlotBounds(inventoryBounds, slotIndex, rowOffset, new Point(1920, 1080));
        }
          /// <summary>
        /// Calculates hotbar slot positions.
        /// </summary>
        /// <param name="hotbarBounds">The hotbar bounds</param>
        /// <param name="slotIndex">Index of the hotbar slot (0-8)</param>
        /// <param name="screenSize">Screen dimensions for responsive calculations</param>
        /// <returns>Rectangle representing the hotbar slot bounds</returns>
        public static Rectangle GetHotbarSlotBounds(Rectangle hotbarBounds, int slotIndex, Point screenSize)
        {
            int slotSize = (int)(screenSize.X * UIConstants.Inventory.SlotSizeRatio);
            int slotWidth = hotbarBounds.Width / UIConstants.Inventory.HotbarSlots;
            int x = hotbarBounds.X + slotIndex * slotWidth + (slotWidth - slotSize) / 2;
            int y = hotbarBounds.Y + (hotbarBounds.Height - slotSize) / 2;
            
            return new Rectangle(x, y, slotSize, slotSize);
        }
        
        /// <summary>
        /// Calculates hotbar slot positions (overload for backward compatibility).
        /// </summary>
        /// <param name="hotbarBounds">The hotbar bounds</param>
        /// <param name="slotIndex">Index of the hotbar slot (0-8)</param>
        /// <returns>Rectangle representing the hotbar slot bounds</returns>
        public static Rectangle GetHotbarSlotBounds(Rectangle hotbarBounds, int slotIndex)
        {
            // Use a default screen size for legacy calls - should be updated to use the new overload
            return GetHotbarSlotBounds(hotbarBounds, slotIndex, new Point(1920, 1080));
        }
        
        /// <summary>
        /// Scales a size value based on screen resolution for responsive design.
        /// </summary>
        /// <param name="baseSize">The base size at 1920x1080 resolution</param>
        /// <param name="currentScreenSize">Current screen dimensions</param>
        /// <param name="baseScreenSize">Base reference screen size (default 1920x1080)</param>
        /// <returns>Scaled size</returns>
        public static int ScaleSize(int baseSize, Point currentScreenSize, Point? baseScreenSize = null)
        {
            var baseSize2D = baseScreenSize ?? new Point(1920, 1080);
            float scaleX = (float)currentScreenSize.X / baseSize2D.X;
            float scaleY = (float)currentScreenSize.Y / baseSize2D.Y;
            float scale = Math.Min(scaleX, scaleY); // Use smaller scale to maintain aspect ratio
            
            return (int)(baseSize * scale);
        }
        
        /// <summary>
        /// Calculates responsive button dimensions.
        /// </summary>
        /// <param name="screenSize">Current screen dimensions</param>
        /// <returns>Point representing button width and height</returns>
        public static Point GetButtonSize(Point screenSize)
        {
            int width = (int)(screenSize.X * UIConstants.Buttons.DefaultButtonWidthRatio);
            int height = (int)(screenSize.Y * UIConstants.Buttons.DefaultButtonHeightRatio);
            return new Point(width, height);
        }
        
        /// <summary>
        /// Calculates responsive button spacing.
        /// </summary>
        /// <param name="screenSize">Current screen dimensions</param>
        /// <returns>Vertical spacing between buttons</returns>
        public static int GetButtonSpacing(Point screenSize)
        {
            return (int)(screenSize.Y * UIConstants.Buttons.ButtonSpacingRatio);
        }
        
        /// <summary>
        /// Calculates responsive menu button top margin.
        /// </summary>
        /// <param name="screenSize">Current screen dimensions</param>
        /// <returns>Top margin for first menu button</returns>
        public static int GetMenuButtonTopMargin(Point screenSize)
        {
            return (int)(screenSize.Y * UIConstants.Buttons.MenuButtonTopMarginRatio);
        }
        
        /// <summary>
        /// Calculates responsive logo bounds.
        /// </summary>
        /// <param name="screenSize">Current screen dimensions</param>
        /// <param name="originalLogoSize">Original logo texture size</param>
        /// <returns>Rectangle representing the logo area</returns>
        public static Rectangle GetLogoBounds(Point screenSize, Point originalLogoSize)
        {
            int targetHeight = (int)(screenSize.Y * UIConstants.Buttons.LogoHeightRatio);
            float aspectRatio = (float)originalLogoSize.X / originalLogoSize.Y;
            int targetWidth = (int)(targetHeight * aspectRatio);
            
            int x = CenterHorizontally(screenSize.X, targetWidth);
            int y = (int)(screenSize.Y * UIConstants.Buttons.LogoTopMarginRatio);
            
            return new Rectangle(x, y, targetWidth, targetHeight);
        }
        
        /// <summary>
        /// Calculates responsive crosshair size.
        /// </summary>
        /// <param name="screenSize">Current screen dimensions</param>
        /// <returns>Size of the crosshair</returns>
        public static int GetCrosshairSize(Point screenSize)
        {
            return (int)(screenSize.X * UIConstants.General.CrosshairSizeRatio);
        }
        
        /// <summary>
        /// Calculates responsive margin.
        /// </summary>
        /// <param name="screenSize">Current screen dimensions</param>
        /// <returns>Default margin size</returns>
        public static int GetDefaultMargin(Point screenSize)
        {
            return (int)(screenSize.X * UIConstants.General.DefaultMarginRatio);
        }
        
        /// <summary>
        /// Calculates responsive padding.
        /// </summary>
        /// <param name="screenSize">Current screen dimensions</param>
        /// <returns>Default padding size</returns>
        public static int GetDefaultPadding(Point screenSize)
        {
            return (int)(screenSize.X * UIConstants.General.DefaultPaddingRatio);
        }
        
        /// <summary>
        /// Scales background image to fit screen while maintaining aspect ratio.
        /// </summary>
        /// <param name="screenSize">Current screen dimensions</param>
        /// <param name="textureSize">Original texture dimensions</param>
        /// <returns>Rectangle for drawing the scaled background</returns>
        public static Rectangle GetScaledBackgroundBounds(Point screenSize, Point textureSize)
        {
            float scaleX = (float)screenSize.X / textureSize.X;
            float scaleY = (float)screenSize.Y / textureSize.Y;
            float scale = Math.Max(scaleX, scaleY); // Use larger scale to fill screen
            
            // Clamp scale to reasonable bounds
            scale = Math.Max(UIConstants.General.BackgroundScaleMin, 
                           Math.Min(UIConstants.General.BackgroundScaleMax, scale));
            
            int scaledWidth = (int)(textureSize.X * scale);
            int scaledHeight = (int)(textureSize.Y * scale);
            
            int x = CenterHorizontally(screenSize.X, scaledWidth);
            int y = CenterVertically(screenSize.Y, scaledHeight);
            
            return new Rectangle(x, y, scaledWidth, scaledHeight);
        }
        
        /// <summary>
        /// Creates a responsive TextBox with standard dimensions.
        /// </summary>
        /// <param name="screenSize">Current screen dimensions</param>
        /// <param name="xRatio">X position as percentage of screen width</param>
        /// <param name="yRatio">Y position as percentage of screen height</param>
        /// <returns>Rectangle for the TextBox</returns>
        public static Rectangle GetTextBoxBounds(Point screenSize, float xRatio, float yRatio)
        {
            int width = (int)(screenSize.X * UIConstants.TextBox.DefaultWidthRatio);
            int height = (int)(screenSize.Y * UIConstants.TextBox.DefaultHeightRatio);
            int x = (int)(screenSize.X * xRatio);
            int y = (int)(screenSize.Y * yRatio);
            
            return new Rectangle(x, y, width, height);
        }
        
        /// <summary>
        /// Creates a responsive SaveGrid with proper positioning.
        /// </summary>
        /// <param name="screenSize">Current screen dimensions</param>
        /// <returns>Rectangle for the SaveGrid area</returns>
        public static Rectangle GetSaveGridBounds(Point screenSize)
        {
            int x = 0;
            int y = (int)(screenSize.Y * UIConstants.SaveGrid.SaveGridTopRatio);
            int width = screenSize.X;
            int height = (int)(screenSize.Y * UIConstants.SaveGrid.SaveGridHeightRatio);
            
            return new Rectangle(x, y, width, height);
        }
        
        /// <summary>
        /// Creates responsive page control button positions for SaveGrid.
        /// </summary>
        /// <param name="screenSize">Current screen dimensions</param>
        /// <param name="isNextButton">True for next button, false for previous</param>
        /// <returns>Rectangle for the button</returns>
        public static Rectangle GetSaveGridButtonBounds(Point screenSize, bool isNextButton)
        {
            Point buttonSize = GetButtonSize(screenSize);
            int spacing = (int)(screenSize.X * UIConstants.SaveGrid.ButtonSpacingRatio);
            int y = (int)(screenSize.Y * UIConstants.SaveGrid.PageControlTopRatio);
            
            int centerX = CenterHorizontally(screenSize.X, buttonSize.X);
            int x = isNextButton ? centerX + spacing : centerX - buttonSize.X - spacing;
            
            return new Rectangle(x, y, buttonSize.X, buttonSize.Y);
        }
    }
}
