using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DevCraft.Utilities
{
    public static class MonitorHelper
    {
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        // System metrics constants
        private const int SM_CXSCREEN = 0;        // Primary monitor width
        private const int SM_CYSCREEN = 1;        // Primary monitor height
        private const int SM_XVIRTUALSCREEN = 76; // Virtual screen left
        private const int SM_YVIRTUALSCREEN = 77; // Virtual screen top
        private const int SM_CXVIRTUALSCREEN = 78; // Virtual screen width
        private const int SM_CYVIRTUALSCREEN = 79; // Virtual screen height
        private const int SM_CMONITORS = 80;      // Number of monitors

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public short dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;
        }

        public struct MonitorInfo
        {
            public string DeviceName;
            public string FriendlyName;
            public int Width;
            public int Height;
            public bool IsPrimary;
            public int Left;
            public int Top;
            public int RefreshRate;
        }

        private static List<MonitorInfo> cachedMonitors = null;

        public static List<MonitorInfo> GetAllMonitors()
        {
            if (cachedMonitors == null)
            {
                cachedMonitors = DetectMonitors();
            }
            return new List<MonitorInfo>(cachedMonitors);
        }

        private static string ParseMonitorName(string deviceString, int displayIndex)
        {
            if (string.IsNullOrEmpty(deviceString))
                return $"Display {displayIndex + 1}";

            // Try to extract meaningful name from device string
            // Example: "Generic PnP Monitor" -> "Display 1"
            // Example: "DELL P2219H" -> "Display 1: DELL P2219H"
            // Example: "Optix G24C" -> "Display 1: Optix G24C"
            
            if (deviceString.Contains("Generic PnP Monitor") || 
                deviceString.Contains("Generic Non-PnP Monitor") ||
                deviceString.Contains("Plug and Play Monitor"))
            {
                return $"Display {displayIndex + 1}";
            }

            // Remove common prefixes/suffixes that aren't useful
            string cleaned = deviceString
                .Replace("(Digital)", "")
                .Replace("(Analog)", "")
                .Replace("Monitor", "")
                .Trim();

            if (string.IsNullOrEmpty(cleaned))
                return $"Display {displayIndex + 1}";

            return $"Display {displayIndex + 1}: {cleaned}";
        }

        private static List<MonitorInfo> DetectMonitors()
        {
            var monitors = new List<MonitorInfo>();
            
            // Console.WriteLine("=== Monitor Detection (Enhanced Method with Real Names) ===");
            
            try
            {
                // Get basic system metrics
                int primaryWidth = GetSystemMetrics(SM_CXSCREEN);
                int primaryHeight = GetSystemMetrics(SM_CYSCREEN);
                int virtualLeft = GetSystemMetrics(SM_XVIRTUALSCREEN);
                int virtualTop = GetSystemMetrics(SM_YVIRTUALSCREEN);
                int virtualWidth = GetSystemMetrics(SM_CXVIRTUALSCREEN);
                int virtualHeight = GetSystemMetrics(SM_CYVIRTUALSCREEN);
                int monitorCount = GetSystemMetrics(SM_CMONITORS);
                
                // Console.WriteLine($"System metrics - Primary: {primaryWidth}x{primaryHeight}, Virtual: {virtualWidth}x{virtualHeight} at ({virtualLeft}, {virtualTop}), Count: {monitorCount}");
                
                // Try to get display devices and their settings using Windows API
                var displayDevices = new List<(DISPLAY_DEVICE device, DEVMODE mode)>();
                for (uint i = 0; ; i++)
                {
                    DISPLAY_DEVICE device = new DISPLAY_DEVICE();
                    device.cb = Marshal.SizeOf(device);
                    
                    if (!EnumDisplayDevices(null, i, ref device, 0))
                        break;
                    
                    // Only get attached displays
                    if ((device.StateFlags & 0x00000001) != 0) // DISPLAY_DEVICE_ATTACHED_TO_DESKTOP
                    {
                        DEVMODE mode = new DEVMODE();
                        mode.dmSize = (short)Marshal.SizeOf(mode);
                        
                        if (EnumDisplaySettings(device.DeviceName, -1, ref mode)) // ENUM_CURRENT_SETTINGS
                        {
                            displayDevices.Add((device, mode));
                            // Console.WriteLine($"Found display: {device.DeviceString} - {mode.dmPelsWidth}x{mode.dmPelsHeight} at ({mode.dmPositionX}, {mode.dmPositionY})");
                        }
                    }
                }
                
                // If we have real display information, use it
                if (displayDevices.Count > 0)
                {
                    for (int i = 0; i < displayDevices.Count; i++)
                    {
                        var (device, mode) = displayDevices[i];
                        bool isPrimary = (mode.dmPositionX == 0 && mode.dmPositionY == 0);
                        
                        // Create a friendly name from the device string
                        string friendlyName = ParseMonitorName(device.DeviceString, i);
                        
                        var monitorInfo = new MonitorInfo
                        {
                            DeviceName = device.DeviceName,
                            FriendlyName = friendlyName,
                            Width = mode.dmPelsWidth,
                            Height = mode.dmPelsHeight,
                            IsPrimary = isPrimary,
                            Left = mode.dmPositionX,
                            Top = mode.dmPositionY,
                            RefreshRate = mode.dmDisplayFrequency
                        };
                        monitors.Add(monitorInfo);
                    }
                }
                else
                {
                    // Fallback to simple method if API fails
                    // Console.WriteLine("Windows API failed, falling back to simple method");
                    
                    // Primary monitor (always at 0,0 in Windows coordinate system)
                    var primaryMonitor = new MonitorInfo
                    {
                        DeviceName = "PRIMARY",
                        FriendlyName = "Display 1: Primary Display",
                        Width = primaryWidth,
                        Height = primaryHeight,
                        IsPrimary = true,
                        Left = 0,
                        Top = 0,
                        RefreshRate = 60
                    };
                    monitors.Add(primaryMonitor);
                    
                    // If we have more than one monitor, try to detect additional ones
                    if (monitorCount > 1)
                    {
                        if (virtualLeft < 0)
                        {
                            // Monitor to the left of primary
                            var leftMonitor = new MonitorInfo
                            {
                                DeviceName = "SECONDARY_LEFT",
                                FriendlyName = "Display 2: Secondary Display (Left)",
                                Width = -virtualLeft,
                                Height = primaryHeight,
                                IsPrimary = false,
                                Left = virtualLeft,
                                Top = 0,
                                RefreshRate = 60
                            };
                            monitors.Add(leftMonitor);
                        }
                        if (virtualWidth > primaryWidth)
                        {
                            // Monitor to the right of primary
                            int rightWidth = virtualWidth - primaryWidth - Math.Max(0, -virtualLeft);
                            if (rightWidth > 0)
                            {
                                var rightMonitor = new MonitorInfo
                                {
                                    DeviceName = "SECONDARY_RIGHT",
                                    FriendlyName = "Display 2: Secondary Display (Right)",
                                    Width = rightWidth,
                                    Height = primaryHeight,
                                    IsPrimary = false,
                                    Left = primaryWidth,
                                    Top = 0,
                                    RefreshRate = 60
                                };
                                monitors.Add(rightMonitor);
                            }
                        }
                    }
                }
                
                // Sort monitors by position (primary first, then left to right, top to bottom)
                monitors.Sort((a, b) => 
                {
                    if (a.IsPrimary && !b.IsPrimary) return -1;
                    if (!a.IsPrimary && b.IsPrimary) return 1;
                    if (a.Left != b.Left) return a.Left.CompareTo(b.Left);
                    return a.Top.CompareTo(b.Top);
                });
                
                // Console.WriteLine($"Detected {monitors.Count} monitors:");
                for (int i = 0; i < monitors.Count; i++)
                {
                    var monitor = monitors[i];
                    // Console.WriteLine($"  Monitor {i}: {monitor.FriendlyName} - {monitor.Width}x{monitor.Height} at ({monitor.Left}, {monitor.Top}) Primary: {monitor.IsPrimary}");
                }            }
            catch (Exception ex)
            {
                // Log the error (in a real application, you'd use a proper logging framework)
                System.Diagnostics.Debug.WriteLine($"Error detecting monitors: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Fallback: just return primary monitor with safe defaults
                monitors.Clear();
                
                try
                {
                    // Try to get at least the primary screen dimensions
                    int primaryWidth = GetSystemMetrics(SM_CXSCREEN);
                    int primaryHeight = GetSystemMetrics(SM_CYSCREEN);
                    
                    // Validate dimensions are reasonable
                    if (primaryWidth > 0 && primaryHeight > 0 && primaryWidth <= 7680 && primaryHeight <= 4320)
                    {
                        monitors.Add(new MonitorInfo
                        {
                            DeviceName = "PRIMARY",
                            FriendlyName = "Display 1: Primary Display",
                            Width = primaryWidth,
                            Height = primaryHeight,
                            IsPrimary = true,
                            Left = 0,
                            Top = 0,
                            RefreshRate = 60
                        });
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid screen dimensions detected: {primaryWidth}x{primaryHeight}");
                    }
                }
                catch (Exception fallbackEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Fallback monitor detection also failed: {fallbackEx.Message}");
                    
                    // Last resort: return a standard resolution
                    monitors.Add(new MonitorInfo
                    {
                        DeviceName = "DEFAULT",
                        FriendlyName = "Display 1: Default (Fallback)",
                        Width = 1920,
                        Height = 1080,
                        IsPrimary = true,
                        Left = 0,
                        Top = 0,
                        RefreshRate = 60
                    });
                }
            }
            
            // Console.WriteLine("=== End Monitor Detection ===");
            return monitors;
        }

        public static string GetMonitorDisplayName(int index)
        {
            var allMonitors = GetAllMonitors();
            if (index >= 0 && index < allMonitors.Count)
            {
                var monitor = allMonitors[index];
                string primaryText = monitor.IsPrimary ? " (Primary)" : "";
                return $"{monitor.FriendlyName} ({monitor.Width}x{monitor.Height}@{monitor.RefreshRate}Hz){primaryText}";
            }
            return "Unknown Monitor";
        }

        public static int GetMonitorCount()
        {
            return GetAllMonitors().Count;
        }

        public static MonitorInfo GetMonitor(int index)
        {
            var allMonitors = GetAllMonitors();
            if (index >= 0 && index < allMonitors.Count)
            {
                return allMonitors[index];
            }
            // Return primary monitor as fallback
            return allMonitors.Find(m => m.IsPrimary);
        }

        public static MonitorInfo GetPrimaryMonitor()
        {
            var allMonitors = GetAllMonitors();
            return allMonitors.Find(m => m.IsPrimary);
        }

        // Helper method to get center point for a window on a specific monitor
        public static Point GetCenterPointForMonitor(int monitorIndex, int windowWidth, int windowHeight)
        {
            var monitor = GetMonitor(monitorIndex);
            var centerX = monitor.Left + (monitor.Width - windowWidth) / 2;
            var centerY = monitor.Top + (monitor.Height - windowHeight) / 2;
              // Console.WriteLine($"Centering {windowWidth}x{windowHeight} window on monitor {monitorIndex}");
            // Console.WriteLine($"  Monitor: {monitor.FriendlyName}");
            // Console.WriteLine($"  Monitor bounds: ({monitor.Left}, {monitor.Top}) {monitor.Width}x{monitor.Height}");
            // Console.WriteLine($"  Calculated center: ({centerX}, {centerY})");
            
            return new Point
            {
                X = centerX,
                Y = centerY
            };
        }

        // Clear cached monitor info (useful if display configuration changes)
        public static void RefreshMonitorCache()
        {
            cachedMonitors = null;
        }
        
        // Validation and safety methods
        
        /// <summary>
        /// Validates if the given monitor index is valid
        /// </summary>
        /// <param name="index">Monitor index to validate</param>
        /// <returns>True if the index is valid</returns>
        public static bool IsValidMonitorIndex(int index)
        {
            try
            {
                var monitors = GetAllMonitors();
                return index >= 0 && index < monitors.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error validating monitor index {index}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validates if the given resolution is supported by any monitor
        /// </summary>
        /// <param name="width">Screen width</param>
        /// <param name="height">Screen height</param>
        /// <returns>True if the resolution is reasonable</returns>
        public static bool IsValidResolution(int width, int height)
        {
            // Basic sanity checks
            if (width <= 0 || height <= 0) return false;
            if (width < 640 || height < 480) return false;  // Minimum reasonable resolution
            if (width > 7680 || height > 4320) return false; // Maximum reasonable resolution (8K)
            
            return true;
        }        /// <summary>
        /// Gets monitor information safely, with fallback to primary monitor
        /// </summary>
        /// <param name="index">Monitor index</param>
        /// <returns>Monitor information, never null</returns>
        public static MonitorInfo GetMonitorSafe(int index)
        {
            try
            {
                if (IsValidMonitorIndex(index))
                {
                    return GetMonitor(index);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting monitor {index}: {ex.Message}");
            }

            // Fallback to primary monitor
            try
            {
                var monitors = GetAllMonitors();
                var primary = monitors.Find(m => m.IsPrimary);
                if (primary.Width > 0) // Check if we got a valid monitor
                {
                    return primary;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting primary monitor: {ex.Message}");
            }

            // Last resort fallback
            return new MonitorInfo
            {
                DeviceName = "SAFE_DEFAULT",
                FriendlyName = "Safe Default Display",
                Width = 1920,
                Height = 1080,
                IsPrimary = true,
                Left = 0,
                Top = 0,
                RefreshRate = 60
            };
        }
    }
}
