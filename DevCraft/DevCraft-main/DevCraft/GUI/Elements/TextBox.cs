using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using DevCraft.Utilities;
using DevCraft.Configuration;


namespace DevCraft.GUI.Elements
{
    class TextBox : GUIElement
    {
        public bool Selected;

        StringBuilder stringBuilder;
        char inputChar;

        GameWindow window;
        SpriteBatch spriteBatch;

        Texture2D texture;
        Texture2D selector;

        SpriteFont font;

        Rectangle rect;
        Vector2 textPosition;


        public TextBox(GameWindow window, GraphicsDevice graphics, SpriteBatch spriteBatch,
                      int x, int y, int width, int height, Texture2D selector, SpriteFont font)
        {
            this.window = window;
            this.spriteBatch = spriteBatch;
            this.selector = selector;
            this.font = font;

            rect = new Rectangle(x, y, width, height);

            texture = new Texture2D(graphics, rect.Width, rect.Height);
            Color[] textureColor = new Color[rect.Width * rect.Height];
            for (int i = 0; i < textureColor.Length; i++)
            {
                textureColor[i] = Color.Black * UIConstants.TextBox.BackgroundAlpha;
            }
            texture.SetData(textureColor);

            stringBuilder = new StringBuilder(UIConstants.TextBox.MaxCharacters);

            // Calculate responsive text padding
            Point screenSize = new Point(graphics.Viewport.Width, graphics.Viewport.Height);
            int textPaddingX = (int)(screenSize.X * UIConstants.TextBox.TextPaddingXRatio);
            int textOffsetY = (int)(height * UIConstants.TextBox.TextOffsetYRatio);
            textPosition = new Vector2(x + textPaddingX, y + textOffsetY);
        }

        public override void Draw()
        {
            if (Inactive)
            {
                return;
            }

            // Draw background with a subtle dark background
            spriteBatch.Draw(texture, rect, Color.White);
            
            // Draw border with better visibility
            if (Selected)
            {
                spriteBatch.Draw(selector, rect, Color.LightBlue * UIConstants.TextBox.BorderAlphaSelected);
            }
            else
            {
                spriteBatch.Draw(selector, rect, Color.Gray * UIConstants.TextBox.BorderAlphaNormal);
            }
            
            // Draw text with better contrast
            spriteBatch.DrawString(font, stringBuilder, textPosition, Color.White);
        }

        public void Update(Point mouseLoc, KeyboardState currentKeyboardState,
            KeyboardState previousKeyboardState, bool leftClick, bool rightClick)
        {
            if (Inactive)
            {
                return;
            }

            if (rect.Contains(mouseLoc))
            {
                if (leftClick)
                {
                    Selected = true;
                }

                if (Selected)
                {
                    window.TextInput += TextInput;
                }

                if (Util.KeyPressed(Keys.Back, currentKeyboardState, previousKeyboardState))
                {
                    RemoveChar();
                }
                else if (font.Characters.Contains(inputChar) && currentKeyboardState.GetPressedKeyCount() > 0 &&
                         Util.KeyPressed(currentKeyboardState.GetPressedKeys()[0], currentKeyboardState, previousKeyboardState))
                {
                    AddChar(inputChar);
                }
            }

            else if (leftClick || rightClick)
            {
                Selected = false;
            }
        }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }

        public void Clear()
        {
            stringBuilder.Length = 0;
        }

        void AddChar(char inputChar)
        {
            // Calculate responsive width buffer for text measurement
            Point screenSize = new Point(1920, 1080); // Default fallback, ideally should be passed in
            int maxWidthBuffer = (int)(screenSize.X * UIConstants.TextBox.MaxWidthBufferRatio);
            
            if (font.MeasureString(stringBuilder).X + maxWidthBuffer < rect.Width)
            {
                stringBuilder.Append(inputChar);
            }
        }

        void RemoveChar()
        {
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Length--;
            }
        }

        void TextInput(object sender, TextInputEventArgs args)
        {
            inputChar = args.Character;
        }
    }
}
