using Microsoft.Xna.Framework;

namespace DevCraft.GUI.Themes
{
    /// <summary>
    /// Modern theme system inspired by Material Design, Unity UI, and Minecraft's design language
    /// Provides consistent colors, gradients, and visual styles across the UI
    /// </summary>
    public static class ModernTheme
    {
        // Primary color palette (inspired by Minecraft but modernized)
        public static class Colors
        {
            // Primary brand colors
            public static readonly Color Primary = new Color(139, 69, 19);          // Minecraft brown
            public static readonly Color PrimaryLight = new Color(160, 82, 45);     // Lighter brown
            public static readonly Color PrimaryDark = new Color(101, 67, 33);      // Darker brown
            
            // Secondary colors
            public static readonly Color Secondary = new Color(34, 139, 34);        // Forest green
            public static readonly Color SecondaryLight = new Color(50, 205, 50);   // Lime green
            public static readonly Color SecondaryDark = new Color(0, 100, 0);      // Dark green
            
            // Neutral colors (with transparency support)
            public static readonly Color Background = new Color(41, 41, 41);        // Dark background
            public static readonly Color BackgroundLight = new Color(55, 55, 55);   // Light background
            public static readonly Color Surface = new Color(66, 66, 66);           // Surface color
            public static readonly Color SurfaceVariant = new Color(77, 77, 77);    // Surface variant
            
            // Text colors
            public static readonly Color OnPrimary = Color.White;
            public static readonly Color OnSecondary = Color.White;
            public static readonly Color OnBackground = Color.White;
            public static readonly Color OnSurface = Color.White;
            public static readonly Color OnSurfaceVariant = new Color(200, 200, 200);
            
            // Semantic colors
            public static readonly Color Success = new Color(76, 175, 80);          // Green
            public static readonly Color Warning = new Color(255, 152, 0);          // Orange
            public static readonly Color Error = new Color(244, 67, 54);            // Red
            public static readonly Color Info = new Color(33, 150, 243);            // Blue
            
            // Interactive states
            public static readonly Color Hover = new Color(255, 255, 255, 25);      // Light overlay
            public static readonly Color Press = new Color(0, 0, 0, 50);            // Dark overlay
            public static readonly Color Focus = new Color(33, 150, 243, 100);      // Blue focus ring
            public static readonly Color Disabled = new Color(128, 128, 128, 100);  // Gray overlay
            
            // Transparency levels
            public static readonly Color Overlay = new Color(0, 0, 0, 128);         // 50% black
            public static readonly Color ModalOverlay = new Color(0, 0, 0, 180);    // 70% black
            public static readonly Color TooltipBackground = new Color(0, 0, 0, 200); // 80% black
        }

        // Shadow and elevation system (Material Design inspired)
        public static class Shadows
        {
            public static readonly Color Shadow1 = new Color(0, 0, 0, 32);          // 1dp elevation
            public static readonly Color Shadow2 = new Color(0, 0, 0, 48);          // 2dp elevation
            public static readonly Color Shadow4 = new Color(0, 0, 0, 64);          // 4dp elevation
            public static readonly Color Shadow8 = new Color(0, 0, 0, 80);          // 8dp elevation
            public static readonly Color Shadow16 = new Color(0, 0, 0, 96);         // 16dp elevation
        }

        // Animation timing (following Material Design motion)
        public static class Animation
        {
            public const float FastDuration = 0.1f;        // 100ms - micro-interactions
            public const float StandardDuration = 0.2f;    // 200ms - standard transitions
            public const float SlowDuration = 0.3f;        // 300ms - complex transitions
            public const float ExtraSlowDuration = 0.5f;   // 500ms - large movements
            
            // Easing curves
            public static readonly AnimationCurve FastOutSlowIn = new AnimationCurve(0.4f, 0.0f, 0.2f, 1.0f);
            public static readonly AnimationCurve LinearOutSlowIn = new AnimationCurve(0.0f, 0.0f, 0.2f, 1.0f);
            public static readonly AnimationCurve FastOutLinearIn = new AnimationCurve(0.4f, 0.0f, 1.0f, 1.0f);
            public static readonly AnimationCurve Linear = new AnimationCurve(0.0f, 0.0f, 1.0f, 1.0f);
        }

        // Component-specific styles
        public static class Button
        {
            public static readonly Color BackgroundPrimary = Colors.Primary;
            public static readonly Color BackgroundSecondary = Colors.Surface;
            public static readonly Color BackgroundTertiary = Color.Transparent;
            
            public static readonly Color TextPrimary = Colors.OnPrimary;
            public static readonly Color TextSecondary = Colors.OnSurface;
            public static readonly Color TextTertiary = Colors.Primary;
            
            public const int BorderRadius = 4;
            public const int Padding = 12;
            public const int MinHeight = 36;
        }

        public static class TextBox
        {
            public static readonly Color Background = Colors.SurfaceVariant;
            public static readonly Color Border = Colors.OnSurfaceVariant;
            public static readonly Color BorderFocused = Colors.Primary;
            public static readonly Color Text = Colors.OnSurface;
            public static readonly Color Placeholder = new Color(Colors.OnSurface, 0.6f);
            
            public const int BorderRadius = 4;
            public const int Padding = 8;
            public const int MinHeight = 32;
        }

        public static class Panel
        {
            public static readonly Color Background = Colors.Surface;
            public static readonly Color Border = Colors.SurfaceVariant;
            
            public const int BorderRadius = 8;
            public const int Padding = 16;
        }

        public static class Tooltip
        {
            public static readonly Color Background = Colors.TooltipBackground;
            public static readonly Color Text = Colors.OnBackground;
            public static readonly Color Shadow = Shadows.Shadow8;
            
            public const int BorderRadius = 4;
            public const int Padding = 8;
            public const float FadeInDuration = Animation.FastDuration;
            public const float FadeOutDuration = Animation.StandardDuration;
        }
    }

    /// <summary>
    /// Simple animation curve representation
    /// </summary>
    public struct AnimationCurve
    {
        public float X1, Y1, X2, Y2;

        public AnimationCurve(float x1, float y1, float x2, float y2)
        {
            X1 = x1; Y1 = y1; X2 = x2; Y2 = y2;
        }

        /// <summary>
        /// Evaluate the curve at time t (0-1)
        /// Simplified cubic bezier evaluation
        /// </summary>
        public float Evaluate(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            
            // Simplified cubic bezier calculation
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            
            return 3 * uu * t * Y1 + 3 * u * tt * Y2 + tt * t;
        }
    }
}
