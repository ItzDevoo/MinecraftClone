using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using DevCraft.Configuration;
using DevCraft.GUI.Core;
using DevCraft.GUI.Utilities;

namespace DevCraft.GUI.Components
{
    /// <summary>
    /// Modern responsive button following Material Design principles
    /// </summary>
    public class ResponsiveButton : UIComponent
    {
        public string Text { get; set; }
        public Action OnClick { get; set; }
        
        private readonly SpriteFont font;
        private readonly Texture2D backgroundTexture;
        private readonly Texture2D hoverTexture;
        
        private bool isHovered;
        private bool isPressed;
        private bool wasPressed;
        
        // Material Design inspired states
        private Color currentColor = UIConstants.Colors.ButtonNormal;
        private readonly Color normalColor = UIConstants.Colors.ButtonNormal;
        private readonly Color hoverColor = UIConstants.Colors.ButtonHover;
        private readonly Color pressedColor = UIConstants.Colors.ButtonPressed;
        private readonly Color disabledColor = UIConstants.Colors.ButtonDisabled;

        public ResponsiveButton(SpriteBatch spriteBatch, GraphicsDevice graphics, Point screenSize,
                              string text, SpriteFont font, Texture2D backgroundTexture, Texture2D hoverTexture,
                              float xRatio, float yRatio, float widthRatio = 0.25f, float heightRatio = 0.06f,
                              Action onClick = null) 
            : base(spriteBatch, graphics, screenSize)
        {
            Text = text;
            this.font = font;
            this.backgroundTexture = backgroundTexture;
            this.hoverTexture = hoverTexture;
            OnClick = onClick;
            
            UpdateLayout();
        }

        protected override void UpdateLayout()
        {
            // Calculate responsive button size and position
            Point buttonSize = UILayoutHelper.GetButtonSize(ScreenSize);
            Point position = GetResponsivePosition(0.5f - (UIConstants.Buttons.DefaultButtonWidthRatio / 2), 0.3f);
            
            Bounds = new Rectangle(position.X, position.Y, buttonSize.X, buttonSize.Y);
        }

        protected override void UpdateInteractions(MouseState mouseState, KeyboardState keyboardState)
        {
            if (!IsEnabled) return;

            bool previousHovered = isHovered;
            isHovered = IsMouseOver(mouseState);
            
            // Material Design hover effects
            if (isHovered && !previousHovered)
            {
                // Hover enter animation would go here
                currentColor = hoverColor;
            }
            else if (!isHovered && previousHovered)
            {
                // Hover exit animation would go here
                currentColor = normalColor;
            }

            // Handle click
            wasPressed = isPressed;
            isPressed = isHovered && mouseState.LeftButton == ButtonState.Pressed;

            if (wasPressed && !isPressed && isHovered)
            {
                // Button was released while hovered - trigger click
                OnClick?.Invoke();
            }

            // Update color based on state
            if (!IsEnabled)
                currentColor = disabledColor;
            else if (isPressed)
                currentColor = pressedColor;
            else if (isHovered)
                currentColor = hoverColor;
            else
                currentColor = normalColor;
        }

        protected override void DrawContent(GameTime gameTime)
        {
            // Draw button background
            SpriteBatch.Draw(backgroundTexture, Bounds, currentColor);
            
            // Draw hover overlay if needed
            if (isHovered && hoverTexture != null)
            {
                SpriteBatch.Draw(hoverTexture, Bounds, Color.White * 0.3f);
            }

            // Draw text centered
            if (!string.IsNullOrEmpty(Text) && font != null)
            {
                Vector2 textSize = font.MeasureString(Text);
                Vector2 textPosition = new Vector2(
                    Bounds.X + (Bounds.Width - textSize.X) / 2,
                    Bounds.Y + (Bounds.Height - textSize.Y) / 2
                );
                
                // Draw text shadow for better readability
                SpriteBatch.DrawString(font, Text, textPosition + Vector2.One, UIConstants.Colors.TextShadow);
                SpriteBatch.DrawString(font, Text, textPosition, UIConstants.Colors.LabelText);
            }
        }

        /// <summary>
        /// Factory method for creating standard menu buttons
        /// </summary>
        public static ResponsiveButton CreateMenuButton(SpriteBatch spriteBatch, GraphicsDevice graphics, 
                                                      Point screenSize, string text, SpriteFont font,
                                                      Texture2D backgroundTexture, Texture2D hoverTexture,
                                                      int menuIndex, Action onClick = null)
        {
            float yPosition = UIConstants.Buttons.MenuButtonTopMarginRatio + 
                            (menuIndex * UIConstants.Buttons.ButtonSpacingRatio);
            
            return new ResponsiveButton(spriteBatch, graphics, screenSize, text, font, 
                                      backgroundTexture, hoverTexture, 0.5f, yPosition, 
                                      UIConstants.Buttons.DefaultButtonWidthRatio,
                                      UIConstants.Buttons.DefaultButtonHeightRatio, onClick);
        }
    }
}
