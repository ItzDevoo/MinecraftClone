using Microsoft.Xna.Framework;

namespace DevCraft.Configuration
{
    /// <summary>
    /// Contains configuration constants for UI layout and positioning.
    /// This eliminates hardcoded magic numbers throughout the codebase.
    /// </summary>
    public static class UIConstants
    {        // Inventory Layout Constants
        public static class Inventory
        {
            // Hotbar dimensions and positioning (relative to screen)
            public const float HotbarWidthRatio = 0.45f;          // Hotbar width as % of screen width
            public const float HotbarHeightRatio = 0.08f;         // Hotbar height as % of screen height
            public const float HotbarBottomMarginRatio = 0.05f;   // Distance from bottom as % of screen height
            public const int HotbarSlots = 9;                     // Number of hotbar slots
            public const float SlotSizeRatio = 0.035f;            // Slot size as % of screen width
            public const float SlotSpacingRatio = 0.051f;         // Distance between slot centers as % of screen width
            public const int SlotBorder = 2;                      // Border around slots
            
            // Main inventory positioning (responsive)
            public const float InventoryLeftRatio = 0.15f;        // Left offset as % of screen width
            public const float InventoryTopRatio = 0.10f;         // Top margin as % of screen height
            public const float InventoryWidthRatio = 0.70f;       // Inventory width as % of screen width
            public const float InventoryHeightRatio = 0.75f;      // Inventory height as % of screen height
            public const int InventoryColumns = 9;                // Number of columns in inventory grid
            
            // Scroller configuration (responsive)
            public const float ScrollerWidthRatio = 0.025f;       // Scroller width as % of screen width
            public const float ScrollerHeightRatio = 0.35f;       // Scroller height as % of screen height
            public const float ScrollerRightMarginRatio = 0.12f;  // Distance from right edge as % of screen width
            public const float ScrollerTopOffsetRatio = 0.025f;   // Top offset for scroller as % of screen height
            public const int ScrollerMinRowsVisible = 5;          // Minimum rows visible in scroller
            public const int ScrollerRowBuffer = 2;               // Buffer rows for scroller calculation
            
            // Selector positioning (responsive)
            public const float SelectorBorderRatio = 0.005f;      // Selector border size as % of screen width
            public const float SelectorInsetRatio = 0.0026f;      // Selector inset as % of screen width
            
            // Text and tooltip settings (responsive)
            public const float TooltipOffsetXRatio = 0.015f;      // Horizontal offset as % of screen width
            public const float TooltipOffsetYRatio = 0.0f;        // Vertical offset as % of screen height
            public const float NameDisplayOffsetYRatio = -0.025f; // Offset for item name display as % of screen height
        }
          // Menu Button Layout Constants
        public static class Buttons
        {
            public const float DefaultButtonWidthRatio = 0.25f;   // Button width as % of screen width
            public const float DefaultButtonHeightRatio = 0.06f;  // Button height as % of screen height
            public const float ButtonSpacingRatio = 0.08f;        // Vertical spacing as % of screen height
            public const float MenuButtonTopMarginRatio = 0.15f;  // Top margin as % of screen height
            public const float LogoHeightRatio = 0.20f;           // Logo height as % of screen height
            public const float LogoTopMarginRatio = 0.05f;        // Logo top margin as % of screen height
        }
          // SaveGrid Layout Constants
        public static class SaveGrid
        {
            public const float SaveSlotWidthRatio = 0.31f;        // Save slot width as % of screen width
            public const float SaveSlotHeightRatio = 0.15f;       // Save slot height as % of screen height
            public const float SaveGridTopRatio = 0.15f;          // Top margin as % of screen height
            public const float SaveGridHeightRatio = 0.24f;       // Grid height as % of screen height
            public const float PageControlTopRatio = 0.40f;       // Page controls top position as % of screen height
            public const float PageLabelLeftRatio = 0.19f;        // Page label left position as % of screen width
            public const float ButtonSpacingRatio = 0.01f;        // Spacing between next/previous buttons
            
            // SaveSlot specific ratios
            public const float SlotIconSizeRatio = 0.033f;        // Save icon size as % of screen width
            public const float SlotIconMarginRatio = 0.005f;      // Save icon margin as % of screen width
            public const float SlotTextOffsetXRatio = 0.038f;     // Text offset from left as % of screen width
            public const float SlotTextOffsetYRatio = 0.006f;     // Text offset from top as % of screen height
            public const float SlotTextSpacingRatio = 0.012f;     // Spacing between text lines as % of screen height
            public const float SlotYSpacingRatio = 0.042f;        // Vertical spacing between slots as % of screen width
            public const float SlotStartOffsetRatio = 0.052f;     // Starting Y offset for first slot as % of screen width
        }
          // TextBox Layout Constants
        public static class TextBox
        {
            public const float DefaultWidthRatio = 0.25f;         // Default width as % of screen width
            public const float DefaultHeightRatio = 0.05f;        // Default height as % of screen height
            public const float TextPaddingXRatio = 0.005f;        // Horizontal text padding as % of screen width
            public const float TextOffsetYRatio = 0.25f;          // Vertical text position offset (% of textbox height)
            public const float MaxWidthBufferRatio = 0.015f;      // Buffer for text width calculation as % of screen width
            public const int MaxCharacters = 30;                  // Maximum characters allowed
            public const float BackgroundAlpha = 0.7f;            // Background transparency
            public const float BorderAlphaSelected = 0.8f;        // Border alpha when selected
            public const float BorderAlphaNormal = 0.6f;          // Border alpha when not selected
        }
          // Scroller Layout Constants  
        public static class Scroller
        {
            public const int ScrollSensitivity = 2;               // Mouse movement threshold for scrolling
            public const int DefaultVisibleItems = 5;             // Default number of visible items
            public const float DefaultWidthRatio = 0.025f;        // Default width as % of screen width
            public const float DefaultHeightRatio = 0.35f;        // Default height as % of screen height
        }
          // General UI Constants
        public static class General
        {
            public const float CrosshairSizeRatio = 0.025f;       // Crosshair size as % of screen width
            public const float ScreenShadingAlpha = 0.5f;         // Alpha for screen overlay when paused
            public const float DefaultMarginRatio = 0.01f;        // Default margin as % of screen width
            public const float DefaultPaddingRatio = 0.005f;      // Default padding as % of screen width
            
            // Background scaling
            public const float BackgroundScaleMin = 1.0f;         // Minimum background scale
            public const float BackgroundScaleMax = 2.0f;         // Maximum background scale
        }
          // Color Constants
        public static class Colors
        {
            public static readonly Color TooltipBackground = Color.Black;
            public static readonly Color TooltipText = Color.White;
            public static readonly Color HotbarBackground = new Color(Color.DarkGray, 0.8f);
            public static readonly Color ScrollerDefault = Color.LightGray;
            
            // UI Component Colors
            public static readonly Color TextShadow = new Color(Color.Black, 0.5f);
            public static readonly Color ButtonNormal = Color.White;
            public static readonly Color ButtonHover = Color.LightGray;
            public static readonly Color ButtonPressed = Color.Gray;
            public static readonly Color ButtonDisabled = Color.DarkGray;
            public static readonly Color TextBoxBackground = new Color(Color.Black, 0.7f);
            public static readonly Color TextBoxBorder = Color.Gray;
            public static readonly Color LabelText = Color.White;
            public static readonly Color ErrorText = Color.Red;
            public static readonly Color WarningText = Color.Orange;
            public static readonly Color InfoText = Color.LightGray;
        }
          // Debug Screen Layout Constants
        public static class Debug
        {
            public const float DebugTextLeftRatio = 0.005f;        // Debug text left margin as % of screen width
            public const float DebugTextTopRatio = 0.005f;         // Debug text top margin as % of screen height
            public const float DebugLineSpacingRatio = 0.015f;     // Line spacing for debug text as % of screen height
        }
    }
}
