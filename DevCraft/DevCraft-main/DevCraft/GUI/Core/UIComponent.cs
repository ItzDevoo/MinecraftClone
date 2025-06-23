using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DevCraft.Configuration;
using DevCraft.GUI.Utilities;

namespace DevCraft.GUI.Core
{
    /// <summary>
    /// Modern UI component base following React/Angular patterns
    /// Provides standardized lifecycle, state management, and responsive behavior
    /// </summary>
    public abstract class UIComponent : IDisposable
    {
        protected SpriteBatch SpriteBatch { get; }
        protected GraphicsDevice Graphics { get; }
        protected Point ScreenSize { get; private set; }
        
        public Rectangle Bounds { get; protected set; }
        public bool IsVisible { get; set; } = true;
        public bool IsEnabled { get; set; } = true;
        public bool IsFocused { get; protected set; }
        
        // Component lifecycle events (React pattern)
        public event Action OnMounted;
        public event Action OnUnmounted;
        public event Action<Point> OnScreenResize;

        protected UIComponent(SpriteBatch spriteBatch, GraphicsDevice graphics, Point screenSize)
        {
            SpriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
            Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            ScreenSize = screenSize;
            
            OnMount();
            OnMounted?.Invoke();
        }

        /// <summary>
        /// Component initialization (componentDidMount in React)
        /// </summary>
        protected virtual void OnMount() { }

        /// <summary>
        /// Handle screen resize (responsive design)
        /// </summary>
        public virtual void HandleScreenResize(Point newScreenSize)
        {
            ScreenSize = newScreenSize;
            UpdateLayout();
            OnScreenResize?.Invoke(newScreenSize);
        }

        /// <summary>
        /// Update component layout based on current screen size
        /// </summary>
        protected abstract void UpdateLayout();

        /// <summary>
        /// Update component state (like React's componentDidUpdate)
        /// </summary>
        public virtual void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState) 
        {
            if (!IsEnabled) return;
            
            UpdateInteractions(mouseState, keyboardState);
        }

        /// <summary>
        /// Handle user interactions
        /// </summary>
        protected virtual void UpdateInteractions(MouseState mouseState, KeyboardState keyboardState) { }

        /// <summary>
        /// Render component (React's render method)
        /// </summary>
        public virtual void Draw(GameTime gameTime)
        {
            if (!IsVisible) return;
            
            DrawContent(gameTime);
        }

        /// <summary>
        /// Actual drawing implementation
        /// </summary>
        protected abstract void DrawContent(GameTime gameTime);

        /// <summary>
        /// Check if mouse is over this component
        /// </summary>
        protected bool IsMouseOver(MouseState mouseState)
        {
            return Bounds.Contains(mouseState.X, mouseState.Y);
        }

        /// <summary>
        /// Get responsive size based on screen percentage
        /// </summary>
        protected Point GetResponsiveSize(float widthRatio, float heightRatio)
        {
            return new Point(
                (int)(ScreenSize.X * widthRatio),
                (int)(ScreenSize.Y * heightRatio)
            );
        }

        /// <summary>
        /// Get responsive position based on screen percentage
        /// </summary>
        protected Point GetResponsivePosition(float xRatio, float yRatio)
        {
            return UILayoutHelper.GetPercentagePosition(ScreenSize, xRatio, yRatio);
        }

        public virtual void Dispose()
        {
            OnUnmounted?.Invoke();
            GC.SuppressFinalize(this);
        }
    }
}
