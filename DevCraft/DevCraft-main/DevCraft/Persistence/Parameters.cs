using System;
using System.IO;

using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using DevCraft.DataModel;

namespace DevCraft.Persistence
{
    public class Parameters
    {
        public bool IsFlying = false;

        public int
        Seed = 0,
        Day = 1,
        Hour = 6,
        Minute = 0;

        public string WorldType = "Default";
        public string SaveName;

        public Vector3
        Position = Vector3.Zero,
        Direction = new(0, -0.5f, -1f);

        public ushort[] Inventory = new ushort[9];

        public DateTime Date = DateTime.Now;

        public Parameters(string saveName)
        {
            Load(saveName);
        }

        public Parameters()
        {
            var rnd = new Random();
            Seed = rnd.Next();
        }

        public void Load(string saveName)
        {
            SaveName = saveName;

            try
            {
                ParametersData data;
                using (StreamReader r = new($"Saves/{saveName}/parameters.json"))
                {
                    string json = r.ReadToEnd();
                    data = JsonConvert.DeserializeObject<ParametersData>(json);
                }

                if (data != null)
                {
                    Seed = data.Seed;
                    IsFlying = data.IsFlying;
                    Position = new Vector3(data.X, data.Y, data.Z);
                    Direction = new Vector3(data.DirX, data.DirY, data.DirZ);
                    Inventory = data.Inventory ?? new ushort[9]; // Prevent null inventory
                    WorldType = data.WorldType ?? "Default";
                    Day = data.Day;
                    Hour = data.Hour;
                    Minute = data.Minute;
                    Date = data.Date;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading parameters for save '{saveName}': {ex.Message}");
                // Initialize with defaults if loading fails
                var rnd = new Random();
                Seed = rnd.Next();
                IsFlying = false;
                Position = Vector3.Zero;
                Direction = new Vector3(0, -0.5f, -1f);
                Inventory = new ushort[9];
                WorldType = "Default";
                Day = 1;
                Hour = 6;
                Minute = 0;
                Date = DateTime.Now;
            }
        }

        public void Save()
        {
            try
            {
                Date = DateTime.Now;

                ParametersData data = new(Seed, IsFlying, Position.X, Position.Y, Position.Z,
                    Direction.X, Direction.Y, Direction.Z, Inventory, WorldType, Day, Hour, Minute, Date);

                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "Saves", SaveName, "parameters.json");

                // Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving parameters for save '{SaveName}': {ex.Message}");
            }
        }
    }
}
