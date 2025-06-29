using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DevCraft.MathUtilities;
using DevCraft.World.Lighting;

namespace DevCraft.Utilities
{
    public enum AxisDirection : byte
    {
        X, Y, Z
    }

    static class Util
    {
        public static string Title(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            return char.ToUpper(str[0]) + str.Substring(1).ToLower();
        }

        public static Faces MaxFace(IEnumerable<LightValue> faceValues)
        {
            Faces face = Faces.YPos;
            LightValue maxValue = LightValue.Null;
            int index = 0;

            foreach (LightValue value in faceValues)
            {
                if (value > maxValue)
                {
                    maxValue = value;
                    face = (Faces)index;
                }
                index++;
            }

            return face;
        }

        public static AxisDirection GetDominantAxis(Vector3 vec)
        {
            float absX = Math.Abs(vec.X);
            float absY = Math.Abs(vec.Y);
            float absZ = Math.Abs(vec.Z);

            if (absX >= absY && absX >= absZ)
            {
                return AxisDirection.X;
            }

            if (absY >= absX && absY >= absZ)
            {
                return AxisDirection.Y;
            }

            return AxisDirection.Z;
        }

        public static Texture2D GetColoredTexture(GraphicsDevice graphics, int width, int height, Color color, float alpha = 1f)
        {
            var texture = new Texture2D(graphics, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new Color(color, alpha);
            }
            texture.SetData(data);

            return texture;
        }

        //Input utility functions
        public static bool KeyPressed(Keys key, KeyboardState currentKeyboardState, KeyboardState previousKeyboardState)
        {
            return currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
        }

        public static bool KeyReleased(Keys key, KeyboardState currentKeyboardState, KeyboardState previousKeyboardState)
        {
            return !currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyDown(key);
        }

        public static bool RightButtonClicked(MouseState currentMouseState, MouseState previousMouseState)
        {
            return currentMouseState.RightButton == ButtonState.Pressed &&
                   previousMouseState.RightButton == ButtonState.Released;
        }

        public static bool LeftButtonClicked(MouseState currentMouseState, MouseState previousMouseState)
        {
            return currentMouseState.LeftButton == ButtonState.Pressed &&
                   previousMouseState.LeftButton == ButtonState.Released;
        }
    }
}
