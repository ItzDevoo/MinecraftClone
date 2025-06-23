using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using DevCraft.Rendering;
using DevCraft.Utilities;

namespace DevCraft.Assets
{
    public class AssetServer(ContentManager content, string rootDirectory)
    {
        readonly ContentManager content = content;
        readonly string rootDirectory = rootDirectory;

        readonly Dictionary<string, Texture2D> menuTextures = [];
        readonly List<Texture2D> blockTextures = [];
        readonly Dictionary<string, int> blockTextureIndices = []; // Maps block names to indices
        readonly List<SpriteFont> fonts = [];

        TexturePackLoader texturePackLoader;

        Effect effect;

        TextureAtlas atlas;

        const int textureSize = 64;

        public Effect Effect => effect.Clone();
        public Texture2D GetBlockTexture(ushort index) => blockTextures[index];
        public Texture2D GetBlockTexture(string blockType) => texturePackLoader?.GetBlockTexture(blockType) ?? GetDefaultBlockTexture(blockType);
        public Texture2D GetMenuTexture(string name) => menuTextures[name];
        public SpriteFont GetFont(int index) => fonts[index];
        public TextureAtlas Atlas => atlas;
        public TexturePackLoader TexturePackLoader => texturePackLoader;

        public void Load(GraphicsDevice graphics)
        {
            LoadBlocks(graphics);
            LoadMenu();
            LoadFonts();
            LoadEffects();
            BuildAtlas(graphics);
            
            // Initialize texture pack loader after everything else is loaded
            texturePackLoader = new TexturePackLoader(graphics, this);
        }

        /// <summary>
        /// Gets a default block texture by name, falling back to stone if not found
        /// </summary>
        /// <param name="blockType">The block type name</param>
        /// <returns>Texture for the block</returns>
        public Texture2D GetDefaultBlockTexture(string blockType)
        {
            try
            {
                if (blockTextureIndices.TryGetValue(blockType, out int index))
                {
                    return blockTextures[index];
                }
                
                // Fall back to stone texture (index 1, since 0 is transparent)
                return blockTextures.Count > 1 ? blockTextures[1] : blockTextures[0];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting default block texture {blockType}: {ex.Message}");
                return blockTextures[0]; // Ultra fallback to transparent texture
            }
        }

        void LoadBlocks(GraphicsDevice graphics)
        {
            // The assets are compiled to the output directory, not the source directory
            string runtimeAssetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rootDirectory);
            string blocksPath = Path.Combine(runtimeAssetsPath, "Textures", "Blocks");
            string[] texturePaths = Directory.GetFiles(blocksPath, "*.xnb");

            blockTextures.Add(Util.GetColoredTexture(graphics, textureSize, textureSize, Color.Transparent, 1));
            blockTextureIndices.Add("air", 0); // Reserve index 0 for air/transparent
            
            int currentIndex = 1;
            foreach (string texturePath in texturePaths)
            {
                string textureName = Path.GetFileNameWithoutExtension(texturePath);
                string assetsTexturePath = Path.Combine("Textures", "Blocks", textureName);
                blockTextures.Add(content.Load<Texture2D>(assetsTexturePath));
                blockTextureIndices.Add(textureName, currentIndex);
                currentIndex++;
            }
        }

        void LoadMenu()
        {
            // The assets are compiled to the output directory, not the source directory
            string runtimeAssetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rootDirectory);
            string menuPath = Path.Combine(runtimeAssetsPath, "Textures", "Menu");
            string[] texturePaths = Directory.GetFiles(menuPath, "*.xnb");

            foreach (string texturePath in texturePaths)
            {
                string textureName = Path.GetFileNameWithoutExtension(texturePath);
                string assetsTexturePath = Path.Combine("Textures", "Menu", textureName);
                menuTextures.Add(textureName, content.Load<Texture2D>(assetsTexturePath));
            }
        }

        void LoadFonts()
        {
            fonts.Add(content.Load<SpriteFont>("Fonts/font14"));
            fonts.Add(content.Load<SpriteFont>("Fonts/font24"));
        }

        void LoadEffects()
        {
            effect = content.Load<Effect>("Effects/BlockEffect");
        }

        void BuildAtlas(GraphicsDevice graphics)
        {
            atlas = new TextureAtlas(textureSize, blockTextures.Count, graphics, GetBlockTexture);
        }
    }
}
