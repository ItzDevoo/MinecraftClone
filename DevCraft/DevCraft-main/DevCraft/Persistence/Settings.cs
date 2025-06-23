using System.IO;
using Newtonsoft.Json;
using DevCraft.DataModel;
using DevCraft.Utilities;

namespace DevCraft.Persistence
{
    static class Settings
    {
        public static int RenderDistance { get; set; } = 8;
        public static string DisplayMode { get; set; } = "Windowed";
        public static int MonitorIndex { get; set; } = GetPrimaryMonitorIndex();

        private static int GetPrimaryMonitorIndex()
        {
            try
            {
                var monitors = MonitorHelper.GetAllMonitors();
                for (int i = 0; i < monitors.Count; i++)
                {
                    if (monitors[i].IsPrimary)
                        return i;
                }
            }
            catch
            {
                // Fallback to monitor 0 if there's any error
            }
            return 0;
        }

        public static void Load()
        {
            if (File.Exists(@"settings.json"))
            {
                SettingsData data;
                using StreamReader r = new("settings.json");

                string json = r.ReadToEnd();
                data = JsonConvert.DeserializeObject<SettingsData>(json);

                RenderDistance = data.RenderDistance;
                DisplayMode = data.DisplayMode ?? "Windowed";
                MonitorIndex = data.MonitorIndex;
            }
            
            // Ensure monitor index is valid after loading
            ValidateMonitorIndex();
        }

        private static void ValidateMonitorIndex()
        {
            try
            {
                int monitorCount = MonitorHelper.GetMonitorCount();
                if (MonitorIndex >= monitorCount || MonitorIndex < 0)
                {
                    MonitorIndex = GetPrimaryMonitorIndex();
                }
            }
            catch
            {
                MonitorIndex = 0; // Fallback
            }
        }

        public static void Save()
        {
            SettingsData data = new(RenderDistance, DisplayMode, MonitorIndex);

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            string path = "settings.json";

            File.WriteAllText(path, json);
        }
    }
}
