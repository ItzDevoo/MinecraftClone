using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.Xna.Framework.Graphics;
using DevCraft.Assets;
using System.Text.Json;

namespace DevCraft.Assets
{
    /// <summary>
    /// Handles loading and managing texture packs for DevCraft
    /// Supports both individual texture files and Minecraft-style resource packs
    /// </summary>
    public class TexturePackLoader
    {
        private readonly GraphicsDevice graphics;
        private readonly AssetServer assetServer;
        private readonly Dictionary<string, Texture2D> loadedTextures;
        private readonly string texturePacksDirectory;
        private readonly string defaultTexturesDirectory;

        public string CurrentTexturePack { get; private set; }
        public List<string> AvailableTexturePacks { get; private set; }

        public TexturePackLoader(GraphicsDevice graphics, AssetServer assetServer)
        {
            this.graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            this.assetServer = assetServer ?? throw new ArgumentNullException(nameof(assetServer));
            
            loadedTextures = new Dictionary<string, Texture2D>();
            texturePacksDirectory = Path.Combine("Assets", "TexturePacks");
            defaultTexturesDirectory = Path.Combine("Assets", "Textures", "Blocks");
            
            // Ensure texture packs directory exists
            Directory.CreateDirectory(texturePacksDirectory);
            
            AvailableTexturePacks = new List<string>();
            CurrentTexturePack = "Default";
            
            RefreshAvailableTexturePacks();
        }

        /// <summary>
        /// Refreshes the list of available texture packs
        /// </summary>
        public void RefreshAvailableTexturePacks()
        {
            AvailableTexturePacks.Clear();
            AvailableTexturePacks.Add("Default");

            try
            {
                if (Directory.Exists(texturePacksDirectory))
                {
                    // Look for .zip files (Minecraft resource packs)
                    var zipFiles = Directory.GetFiles(texturePacksDirectory, "*.zip");
                    foreach (var zipFile in zipFiles)
                    {
                        string packName = Path.GetFileNameWithoutExtension(zipFile);
                        AvailableTexturePacks.Add(packName);
                    }

                    // Look for directories (extracted texture packs)
                    var directories = Directory.GetDirectories(texturePacksDirectory);
                    foreach (var dir in directories)
                    {
                        string packName = Path.GetFileName(dir);
                        if (!AvailableTexturePacks.Contains(packName))
                        {
                            AvailableTexturePacks.Add(packName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing texture packs: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads a texture pack by name
        /// </summary>
        /// <param name="packName">Name of the texture pack to load</param>
        /// <returns>True if successfully loaded</returns>
        public bool LoadTexturePack(string packName)
        {
            try
            {
                if (packName == "Default")
                {
                    CurrentTexturePack = "Default";
                    ClearLoadedTextures();
                    return true;
                }

                string packPath = Path.Combine(texturePacksDirectory, packName);
                
                // Check if it's a zip file
                string zipPath = packPath + ".zip";
                if (File.Exists(zipPath))
                {
                    return LoadFromZip(zipPath, packName);
                }
                
                // Check if it's a directory
                if (Directory.Exists(packPath))
                {
                    return LoadFromDirectory(packPath, packName);
                }

                System.Diagnostics.Debug.WriteLine($"Texture pack not found: {packName}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading texture pack {packName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets a block texture, using the current texture pack or falling back to default
        /// </summary>
        /// <param name="blockType">Type of block</param>
        /// <returns>Texture for the block</returns>
        public Texture2D GetBlockTexture(string blockType)
        {
            try
            {
                // First try to get from loaded texture pack
                if (loadedTextures.TryGetValue(blockType, out Texture2D packedTexture))
                {
                    return packedTexture;
                }

                // Fall back to default asset server
                return assetServer.GetBlockTexture(blockType);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting block texture {blockType}: {ex.Message}");
                return assetServer.GetBlockTexture("stone"); // Ultra fallback
            }
        }

        private bool LoadFromZip(string zipPath, string packName)
        {
            try
            {
                using var archive = ZipFile.OpenRead(zipPath);
                
                // Look for Minecraft-style paths first
                var textureEntries = new Dictionary<string, ZipArchiveEntry>();
                
                foreach (var entry in archive.Entries)
                {
                    // Minecraft resource pack structure: assets/minecraft/textures/block/
                    if (entry.FullName.StartsWith("assets/minecraft/textures/block/") && 
                        entry.FullName.EndsWith(".png"))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(entry.Name);
                        textureEntries[fileName] = entry;
                    }
                    // Also check root level for simpler packs
                    else if (entry.FullName.EndsWith(".png") && !entry.FullName.Contains("/"))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(entry.Name);
                        if (!textureEntries.ContainsKey(fileName))
                        {
                            textureEntries[fileName] = entry;
                        }
                    }
                }

                // Load textures from zip
                foreach (var kvp in textureEntries)
                {
                    try
                    {
                        using var stream = kvp.Value.Open();
                        using var memoryStream = new MemoryStream();
                        stream.CopyTo(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        
                        var texture = Texture2D.FromStream(graphics, memoryStream);
                        loadedTextures[kvp.Key] = texture;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading texture {kvp.Key} from zip: {ex.Message}");
                    }
                }

                CurrentTexturePack = packName;
                System.Diagnostics.Debug.WriteLine($"Loaded texture pack: {packName} ({loadedTextures.Count} textures)");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading zip texture pack: {ex.Message}");
                return false;
            }
        }

        private bool LoadFromDirectory(string directoryPath, string packName)
        {
            try
            {
                // Look for PNG files in the directory
                var pngFiles = Directory.GetFiles(directoryPath, "*.png", SearchOption.AllDirectories);
                
                foreach (var pngFile in pngFiles)
                {
                    try
                    {
                        string fileName = Path.GetFileNameWithoutExtension(pngFile);
                        using var fileStream = File.OpenRead(pngFile);
                        var texture = Texture2D.FromStream(graphics, fileStream);
                        loadedTextures[fileName] = texture;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading texture {pngFile}: {ex.Message}");
                    }
                }

                CurrentTexturePack = packName;
                System.Diagnostics.Debug.WriteLine($"Loaded texture pack: {packName} ({loadedTextures.Count} textures)");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading directory texture pack: {ex.Message}");
                return false;
            }
        }

        private void ClearLoadedTextures()
        {
            foreach (var texture in loadedTextures.Values)
            {
                texture?.Dispose();
            }
            loadedTextures.Clear();
        }

        public void Dispose()
        {
            ClearLoadedTextures();
        }
    }
}
