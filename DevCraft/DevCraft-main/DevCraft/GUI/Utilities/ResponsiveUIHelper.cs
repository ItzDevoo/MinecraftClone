using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DevCraft.Configuration;

namespace DevCraft.GUI.Utilities
{
    /// <summary>
    /// Responsive UI utilities following Material Design and modern UI frameworks like React/Angular
    /// Provides industry-standard responsive design patterns for game UI
    /// </summary>
    public static class ResponsiveUIHelper
    {
        /// <summary>
        /// Calculate responsive font size based on screen resolution
        /// Follows Material Design Typography scale
        /// </summary>
        public static float GetResponsiveFontSize(Point screenSize, FontSize size)
        {
            float baseScale = Math.Min(screenSize.X / 1920f, screenSize.Y / 1080f);
            
            return size switch
            {
                FontSize.Caption => Math.Max(10f, 12f * baseScale),      // Small text
                FontSize.Body => Math.Max(12f, 14f * baseScale),         // Default body text
                FontSize.Subtitle => Math.Max(14f, 16f * baseScale),     // Subtitles
                FontSize.Title => Math.Max(18f, 20f * baseScale),        // Section titles
                FontSize.Headline => Math.Max(20f, 24f * baseScale),     // Main headings
                FontSize.Display => Math.Max(24f, 32f * baseScale),      // Large display text
                _ => Math.Max(12f, 14f * baseScale)
            };
        }

        /// <summary>
        /// Get responsive breakpoint for current screen size
        /// Follows Bootstrap/Material Design breakpoints
        /// </summary>
        public static ScreenBreakpoint GetScreenBreakpoint(Point screenSize)
        {
            int width = screenSize.X;
            
            return width switch
            {
                < 768 => ScreenBreakpoint.Mobile,      // Phone
                < 1024 => ScreenBreakpoint.Tablet,     // Tablet
                < 1440 => ScreenBreakpoint.Desktop,    // Standard desktop
                < 1920 => ScreenBreakpoint.Large,      // Large desktop
                _ => ScreenBreakpoint.ExtraLarge        // 4K+
            };
        }

        /// <summary>
        /// Get responsive spacing based on screen size and context
        /// Provides consistent spacing across all screen sizes
        /// </summary>
        public static int GetResponsiveSpacing(Point screenSize, SpacingSize size)
        {
            float scale = Math.Min(screenSize.X / 1920f, screenSize.Y / 1080f);
            
            return size switch
            {
                SpacingSize.XSmall => Math.Max(2, (int)(4 * scale)),
                SpacingSize.Small => Math.Max(4, (int)(8 * scale)),
                SpacingSize.Medium => Math.Max(8, (int)(16 * scale)),
                SpacingSize.Large => Math.Max(12, (int)(24 * scale)),
                SpacingSize.XLarge => Math.Max(16, (int)(32 * scale)),
                _ => Math.Max(8, (int)(16 * scale))
            };
        }

        /// <summary>
        /// Check if text will fit in the given bounds and suggest font size adjustment
        /// </summary>
        public static bool WillTextFit(string text, SpriteFont font, Rectangle bounds, out float suggestedScale)
        {
            Vector2 textSize = font.MeasureString(text);
            suggestedScale = 1.0f;
            
            if (textSize.X > bounds.Width || textSize.Y > bounds.Height)
            {
                float scaleX = bounds.Width / textSize.X;
                float scaleY = bounds.Height / textSize.Y;
                suggestedScale = Math.Min(scaleX, scaleY) * 0.9f; // 90% to provide padding
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Get optimal icon size for current screen resolution
        /// </summary>
        public static int GetIconSize(Point screenSize, IconSize size)
        {
            float scale = Math.Min(screenSize.X / 1920f, screenSize.Y / 1080f);
            
            return size switch
            {
                IconSize.Small => Math.Max(16, (int)(16 * scale)),
                IconSize.Medium => Math.Max(24, (int)(24 * scale)),
                IconSize.Large => Math.Max(32, (int)(32 * scale)),
                IconSize.XLarge => Math.Max(48, (int)(48 * scale)),
                _ => Math.Max(24, (int)(24 * scale))
            };
        }
    }

    public enum FontSize
    {
        Caption,
        Body,
        Subtitle,
        Title,
        Headline,
        Display
    }

    public enum ScreenBreakpoint
    {
        Mobile,
        Tablet,
        Desktop,
        Large,
        ExtraLarge
    }

    public enum SpacingSize
    {
        XSmall,
        Small,
        Medium,
        Large,
        XLarge
    }

    public enum IconSize
    {
        Small,
        Medium,
        Large,
        XLarge
    }
}
