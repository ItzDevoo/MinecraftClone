using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DevCraft.GUI.Utilities
{
    /// <summary>
    /// Manages UI resources like textures to prevent memory leaks and improve performance.
    /// Implements proper disposal patterns and resource caching.
    /// </summary>
    public class UIResourceManager : IDisposable
    {
        private readonly GraphicsDevice graphicsDevice;
        private readonly Dictionary<string, Texture2D> textureCache;
        private bool disposed = false;
        
        public UIResourceManager(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            textureCache = new Dictionary<string, Texture2D>();
        }
        
        /// <summary>
        /// Creates or retrieves a cached colored texture.
        /// </summary>
        /// <param name="key">Unique identifier for the texture</param>
        /// <param name="width">Width of the texture</param>
        /// <param name="height">Height of the texture</param>
        /// <param name="color">Color to fill the texture with</param>
        /// <returns>The created or cached texture</returns>
        public Texture2D GetColoredTexture(string key, int width, int height, Color color)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(UIResourceManager));
                
            if (textureCache.TryGetValue(key, out Texture2D cachedTexture))
            {
                return cachedTexture;
            }
            
            var texture = new Texture2D(graphicsDevice, width, height);
            var colorData = new Color[width * height];
            Array.Fill(colorData, color);
            texture.SetData(colorData);
            
            textureCache[key] = texture;
            return texture;
        }
        
        /// <summary>
        /// Creates a standard black texture for UI backgrounds.
        /// </summary>
        /// <param name="width">Width of the texture</param>
        /// <param name="height">Height of the texture</param>
        /// <returns>Black texture for UI backgrounds</returns>
        public Texture2D GetBlackTexture(int width = 20, int height = 10)
        {
            string key = $"black_{width}x{height}";
            return GetColoredTexture(key, width, height, Color.Black);
        }
        
        /// <summary>
        /// Creates a standard light gray texture for scrollers and UI elements.
        /// </summary>
        /// <param name="width">Width of the texture</param>
        /// <param name="height">Height of the texture</param>
        /// <returns>Light gray texture for UI elements</returns>
        public Texture2D GetScrollerTexture(int width = 10, int height = 10)
        {
            string key = $"scroller_{width}x{height}";
            return GetColoredTexture(key, width, height, Color.LightGray);
        }
        
        /// <summary>
        /// Removes a texture from the cache and disposes it.
        /// </summary>
        /// <param name="key">Key of the texture to remove</param>
        public void RemoveTexture(string key)
        {
            if (textureCache.TryGetValue(key, out Texture2D texture))
            {
                texture?.Dispose();
                textureCache.Remove(key);
            }
        }
        
        /// <summary>
        /// Clears all cached textures and disposes them.
        /// </summary>
        public void ClearCache()
        {
            foreach (var texture in textureCache.Values)
            {
                texture?.Dispose();
            }
            textureCache.Clear();
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                ClearCache();
                disposed = true;
            }
        }
        
        ~UIResourceManager()
        {
            Dispose(false);
        }
    }
}
