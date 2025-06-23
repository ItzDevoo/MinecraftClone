using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DevCraft.Persistence
{
    class Save
    {
        public string Name;
        public Parameters Parameters;
        public Texture2D Icon;


        public Save(Texture2D icon, string name, Parameters parameters)
        {
            Icon = icon;
            Name = name;
            Parameters = parameters;

            Parameters.SaveName = Name;
        }

        public Save(string name, Parameters parameters)
        {
            Name = name;
            Parameters = parameters;

            Parameters.SaveName = Name;
        }

        public void Clear()
        {
            try
            {
                // Delete the old SQLite database file if it exists
                string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "Saves", Name, "data.db");
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                    Console.WriteLine($"Deleted old database file: {dbPath}");
                }

                // Clear all chunk files 
                string chunksDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Saves", Name, "chunks");
                if (Directory.Exists(chunksDirectory))
                {
                    Directory.Delete(chunksDirectory, true);
                    Console.WriteLine($"Cleared chunks directory: {chunksDirectory}");
                }

                // Preserve important save metadata
                string saveName = Parameters.SaveName;
                string worldType = Parameters.WorldType;
                int seed = Parameters.Seed;
                DateTime date = Parameters.Date;

                Parameters = new Parameters
                {
                    SaveName = saveName,
                    WorldType = worldType,
                    Seed = seed,
                    Date = date
                };

                Parameters.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing save '{Name}': {ex.Message}");
            }
        }

        public static List<Save> LoadAll(GraphicsDevice graphics)
        {
            List<Save> saves = [];
            string[] saveNames = Directory.Exists("Saves") ? Directory.GetDirectories("Saves") : [];

            for (int i = 0; i < saveNames.Length; i++)
            {
                if (!File.Exists($"{saveNames[i]}/parameters.json"))
                {
                    continue;
                }

                string saveName = saveNames[i].Split('\\')[1];
                saves.Add(Load(graphics, saveName));
            }

            return saves;
        }

        public static Save Load(GraphicsDevice graphics, string name)
        {
            Texture2D icon = null;
            
            try
            {
                string iconPath = @$"Saves\{name}\save_icon.png";
                if (File.Exists(iconPath))
                {
                    using FileStream fileStream = new(iconPath, FileMode.Open);
                    icon = Texture2D.FromStream(graphics, fileStream);
                }
            }
            catch (Exception)
            {
                // Icon loading failed, will use fallback below
            }
            
            // If icon loading failed or file doesn't exist, create a fallback icon
            if (icon == null)
            {
                icon = new Texture2D(graphics, 64, 64);
                Color[] fallbackData = new Color[64 * 64];
                for (int i = 0; i < fallbackData.Length; i++)
                {
                    fallbackData[i] = Color.Gray; // Gray fallback icon
                }
                icon.SetData(fallbackData);
            }

            return new Save(icon, name, new Parameters(name));
        }
    }
}
