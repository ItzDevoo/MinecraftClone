using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DevCraft.Persistence;
using DevCraft.Configuration;

namespace DevCraft.GUI.Components
{
    class SaveSlot
    {
        SpriteBatch spriteBatch;

        Texture2D selector;

        SpriteFont font;

        Rectangle rect;
        Point screenSize;


        public SaveSlot(SpriteBatch spriteBatch, int x, int width, int height,
                        Texture2D selector, SpriteFont font, Point screenSize)
        {
            this.spriteBatch = spriteBatch;
            this.selector = selector;
            this.font = font;
            this.screenSize = screenSize;

            rect = new Rectangle(x, 0, width, height);
        }

        public void DrawAt(int yOffset, Save save, bool selected)
        {
            // Calculate responsive positions
            int iconSize = (int)(screenSize.X * UIConstants.SaveGrid.SlotIconSizeRatio);
            int iconMargin = (int)(screenSize.X * UIConstants.SaveGrid.SlotIconMarginRatio);
            int textOffsetX = (int)(screenSize.X * UIConstants.SaveGrid.SlotTextOffsetXRatio);
            int textOffsetY = (int)(screenSize.Y * UIConstants.SaveGrid.SlotTextOffsetYRatio);
            int textSpacing = (int)(screenSize.Y * UIConstants.SaveGrid.SlotTextSpacingRatio);
            
            int actualY = (int)(screenSize.Y * UIConstants.SaveGrid.SaveGridTopRatio) + 
                         (int)(screenSize.X * UIConstants.SaveGrid.SlotStartOffsetRatio) + 
                         yOffset * (int)(screenSize.X * UIConstants.SaveGrid.SlotYSpacingRatio);

            Vector2 namePosition = new(rect.X + textOffsetX, actualY + textOffsetY);
            Vector2 datePosition = new(rect.X + textOffsetX, actualY + textOffsetY + textSpacing);

            rect.Y = actualY;

            // Only draw the icon if it's not null
            if (save.Icon != null)
            {
                spriteBatch.Draw(save.Icon, new Rectangle(rect.X + iconMargin, actualY + iconMargin, iconSize, iconSize), Color.White);
            }
            
            spriteBatch.DrawString(font, save.Name, namePosition, Color.White);
            spriteBatch.DrawString(font, "Last modified on: " + save.Parameters.Date, datePosition, Color.DarkGray);

            if (selected)
            {
                spriteBatch.Draw(selector, rect, Color.White);
            }
            else
            {
                spriteBatch.Draw(selector, rect, Color.DarkGray);
            }
        }

        public bool ContainsAt(int yOffset, Point point)
        {
            int actualY = (int)(screenSize.Y * UIConstants.SaveGrid.SaveGridTopRatio) + 
                         (int)(screenSize.X * UIConstants.SaveGrid.SlotStartOffsetRatio) + 
                         yOffset * (int)(screenSize.X * UIConstants.SaveGrid.SlotYSpacingRatio);
            rect.Y = actualY;

            return rect.Contains(point);
        }
    }
}
