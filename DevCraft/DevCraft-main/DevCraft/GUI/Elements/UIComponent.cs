using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DevCraft.Configuration;

namespace DevCraft.GUI.Elements
{    /// <summary>
    /// Base class for all UI components providing common functionality and standardized behavior
    /// </summary>
    internal abstract class UIComponent : GUIElement, IDisposable
    {
        protected SpriteBatch spriteBatch;
        protected GraphicsDevice graphics;
        protected SpriteFont font;
        protected Rectangle bounds;
        protected Vector2 position;
        protected Color textColor;
        protected Color backgroundColor;
        protected bool isHovered;
        protected bool isPressed;
        protected bool isDisposed;

        // Common UI properties
        public Vector2 Position 
        { 
            get => position; 
            set 
            { 
                position = value;
                UpdateBounds();
            } 
        }

        public Rectangle Bounds => bounds;
        public Color TextColor { get => textColor; set => textColor = value; }
        public Color BackgroundColor { get => backgroundColor; set => backgroundColor = value; }
        public bool IsHovered => isHovered;
        public bool IsPressed => isPressed;

        protected UIComponent(SpriteBatch spriteBatch, GraphicsDevice graphics, SpriteFont font, 
                            Vector2 position, Color textColor, Color backgroundColor)
        {
            this.spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
            this.graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            this.font = font ?? throw new ArgumentNullException(nameof(font));
            this.position = position;
            this.textColor = textColor;
            this.backgroundColor = backgroundColor;
            
            UpdateBounds();
        }

        /// <summary>
        /// Updates the component's bounds based on current position and content
        /// </summary>
        protected abstract void UpdateBounds();

        /// <summary>
        /// Handles mouse hover state
        /// </summary>
        /// <param name="mousePosition">Current mouse position</param>
        /// <returns>True if mouse is over this component</returns>
        public virtual bool UpdateHoverState(Point mousePosition)
        {
            bool wasHovered = isHovered;
            isHovered = bounds.Contains(mousePosition) && !Inactive;
            
            // Return true if hover state changed
            return wasHovered != isHovered;
        }

        /// <summary>
        /// Handles mouse press state
        /// </summary>
        /// <param name="mousePosition">Current mouse position</param>
        /// <param name="isLeftButtonPressed">Whether left mouse button is pressed</param>
        /// <returns>True if this component was clicked</returns>
        public virtual bool UpdatePressState(Point mousePosition, bool isLeftButtonPressed)
        {
            bool wasPressed = isPressed;
            isPressed = isHovered && isLeftButtonPressed && !Inactive;
            
            // Return true if component was just clicked (pressed but wasn't pressed before)
            return isPressed && !wasPressed;
        }

        /// <summary>
        /// Measures the size of text using the component's font
        /// </summary>
        /// <param name="text">Text to measure</param>
        /// <returns>Size of the text in pixels</returns>
        protected Vector2 MeasureText(string text)
        {
            return string.IsNullOrEmpty(text) ? Vector2.Zero : font.MeasureString(text);
        }

        /// <summary>
        /// Calculates centered text position within bounds
        /// </summary>
        /// <param name="text">Text to center</param>
        /// <returns>Position to draw text at for centering</returns>
        protected Vector2 GetCenteredTextPosition(string text)
        {
            Vector2 textSize = MeasureText(text);
            return new Vector2(
                bounds.X + (bounds.Width - textSize.X) / 2,
                bounds.Y + (bounds.Height - textSize.Y) / 2
            );
        }

        /// <summary>
        /// Draws a background texture or color for the component
        /// </summary>
        /// <param name="texture">Texture to draw, or null for solid color</param>
        protected virtual void DrawBackground(Texture2D texture = null)
        {
            if (texture != null)
            {
                spriteBatch.Draw(texture, bounds, Color.White);
            }
            else
            {
                // Draw solid color background
                spriteBatch.Draw(CreateSolidTexture(graphics, 1, 1, backgroundColor), bounds, backgroundColor);
            }
        }

        /// <summary>
        /// Draws text with optional shadow/outline effect
        /// </summary>
        /// <param name="text">Text to draw</param>
        /// <param name="textPosition">Position to draw at</param>
        /// <param name="color">Text color</param>
        /// <param name="drawShadow">Whether to draw a shadow</param>
        protected virtual void DrawText(string text, Vector2 textPosition, Color color, bool drawShadow = false)
        {
            if (string.IsNullOrEmpty(text)) return;

            // Draw shadow first if requested
            if (drawShadow)
            {
                spriteBatch.DrawString(font, text, textPosition + Vector2.One, UIConstants.Colors.TextShadow);
            }

            // Draw main text
            spriteBatch.DrawString(font, text, textPosition, color);
        }

        /// <summary>
        /// Creates a 1x1 solid color texture for drawing backgrounds
        /// </summary>
        private static Texture2D CreateSolidTexture(GraphicsDevice graphics, int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(graphics, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = color;
            }
            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Standard update method combining hover and press state updates
        /// </summary>
        /// <param name="mousePosition">Current mouse position</param>
        /// <param name="isLeftButtonPressed">Whether left mouse button is pressed</param>
        /// <returns>True if component was clicked this frame</returns>
        public override bool Clicked(Point mousePosition, bool isLeftButtonPressed)
        {
            UpdateHoverState(mousePosition);
            return UpdatePressState(mousePosition, isLeftButtonPressed);
        }

        #region IDisposable Implementation

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed && disposing)
            {
                // Dispose managed resources
                // Note: We don't dispose SpriteBatch, GraphicsDevice, or SpriteFont 
                // as they are shared resources managed elsewhere
                isDisposed = true;
            }
        }

        #endregion
    }
}
