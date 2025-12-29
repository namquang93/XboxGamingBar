using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace XboxGamingBarHelper.Windows
{
    internal class User32
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct DEVMODE
        {
            private const int CCHDEVICENAME = 32;
            private const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
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
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
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

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly List<string> ProtectedProcesses = new List<string>() { "parsecd", "Taskmgr" };

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out IntPtr lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        // Delegate for the EnumWindows callback function
        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        // DllImport for the EnumWindows function
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        // DllImport for IsWindowVisible
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        // DllImport for GetShellWindow
        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int ChangeDisplaySettings(ref DEVMODE lpDevMode, int dwFlags);

        private static int GetWindowProcessId(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero)
            {
                Logger.Error("Can't get process id from invalid window handle.");
                return -1;
            }

            GetWindowThreadProcessId(windowHandle, out IntPtr windowProcessId);
            if (windowProcessId == IntPtr.Zero)
            {
                Logger.Error("Can't get window process id.");
                return -1;
            }

            return (int)windowProcessId;
        }

        public static string GetWindowTitle(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero)
            {
                Logger.Error("Can't get window title from invalid window handle.");
                return string.Empty;
            }

            var length = GetWindowTextLength(windowHandle);
            if (length == 0)
            {
                //Logger.Error($"Window doesn't have window title??");
                return string.Empty;
            }

            var sb = new StringBuilder(length + 1);
            GetWindowText(windowHandle, sb, sb.Capacity);

            return sb.ToString();
        }

        public static void GetOpenWindows(IDictionary<int, ProcessWindow> windows)
        {
            var shellWindow = GetShellWindow();
            var foregroundWindowProcessId = GetWindowProcessId(GetForegroundWindow());
            if (windows == null)
            {
                windows = new Dictionary<int, ProcessWindow>();
            }
            else
            {
                windows.Clear();
            }

            EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                // Exclude the shell window itself
                if (hWnd == shellWindow)
                {
                    Logger.Debug("Skip window because it's the shell window.");
                    return true;
                }

                // Exclude invisible windows
                if (!IsWindowVisible(hWnd))
                {
                    return true;
                }

                var processId = GetWindowProcessId(hWnd);
                var windowTitle = GetWindowTitle(hWnd);
                if (processId == -1)
                {
                    Logger.Warn($"Window doesn't have process id???");
                    return true; // Continue enumeration
                }

                var process = Process.GetProcessById(processId);
                if (ProtectedProcesses.Contains(process.ProcessName))
                {
                    Logger.Debug($"Ignore protected process {process.ProcessName}");
                    return true;
                }

                var fileName = string.Empty;
                try
                {
                    fileName = process.MainModule.FileName;
                }
                catch (Exception e)
                {
                    Logger.Warn($"Can't get file name {e.Message} of process {process.ProcessName}.");
                }
                windows[processId] = new ProcessWindow(processId, hWnd, windowTitle, process.ProcessName, fileName, processId == foregroundWindowProcessId);
                return true; // Continue enumeration
            }, 0);
        }

        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int CDS_UPDATEREGISTRY = 0x01;
        private const int CDS_TEST = 0x02;
        private const int DISP_CHANGE_SUCCESSFUL = 0;
        private const int DM_PELSWIDTH = 0x00080000;
        private const int DM_PELSHEIGHT = 0x00100000;
        private const int DM_DISPLAYFREQUENCY = 0x00400000;
        private const int DM_DISPLAYORIENTATION = 0x00000080;

        public static int GetCurrentRefreshRate()
        {
            DEVMODE vDevMode = new DEVMODE();
            vDevMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref vDevMode))
            {
                Logger.Info($"Current refresh rate: {vDevMode.dmDisplayFrequency}Hz");
                return vDevMode.dmDisplayFrequency;
            }
            Logger.Error("Failed to get current refresh rate.");
            return 0; // failed
        }

        public static List<int> GetSupportedRefreshRates()
        {
            List<int> refreshRates = new List<int>();
            DEVMODE devMode = new DEVMODE();
            devMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            int modeIndex = 0;
            (int nativeWidth, int nativeHeight) = GetNativeResolution();
            float aspectRatio = (float)nativeWidth / nativeHeight;
            const float tolerance = 0.001f;

            while (EnumDisplaySettings(null, modeIndex++, ref devMode))
            {
                int rate = devMode.dmDisplayFrequency;
                float refreshRateAspectRatio = (float)devMode.dmPelsWidth / devMode.dmPelsHeight;
                if (rate > 0 && !refreshRates.Contains(rate) && Math.Abs(aspectRatio - refreshRateAspectRatio) <= tolerance)
                {
                    refreshRates.Add(rate);
                }
                else
                {
                    Logger.Debug($"Skipping refresh rate {rate}Hz at resolution {devMode.dmPelsWidth}x{devMode.dmPelsHeight} because it's not compatible with current aspect ratio {refreshRateAspectRatio} !~ {aspectRatio}");
                }
            }

            refreshRates.Sort();
            foreach (var refreshRate in refreshRates)
            {
                Logger.Info($"Found refresh rate {refreshRate}");
            }
            return refreshRates;
        }

        /// <summary>
        /// Set monitor refresh rate to a supported value.
        /// </summary>
        public static bool SetRefreshRate(int targetRate)
        {
            /*var supported = GetSupportedRefreshRates();
            if (!supported.Contains(targetRate))
            {
                Console.WriteLine($"Error: {targetRate}Hz is not supported. Supported rates: {string.Join(", ", supported)}");
                return false;
            }*/

            DEVMODE mode = new DEVMODE { dmSize = (short)Marshal.SizeOf(typeof(DEVMODE)) };

            if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref mode))
            {
                Logger.Info("Error: Could not retrieve current display settings.");
                return false;
            }

            mode.dmDisplayFrequency = targetRate;
            mode.dmFields |= DM_DISPLAYFREQUENCY;

            // Test before applying
            int testResult = ChangeDisplaySettings(ref mode, CDS_TEST);
            if (testResult != DISP_CHANGE_SUCCESSFUL)
            {
                Logger.Info($"Test failed: {targetRate}Hz not valid on this mode.");
                return false;
            }

            // Apply permanently
            int result = ChangeDisplaySettings(ref mode, CDS_UPDATEREGISTRY);
            if (result == DISP_CHANGE_SUCCESSFUL)
            {
                Logger.Info($"Sucessfully set refresh rate to {targetRate}Hz.");
                return true;
            }
            else
            {
                Logger.Info($"Failed to set refresh rate to {targetRate}Hz (error code {result}).");
                return false;
            }
        }

        static (int width, int height) GetNativeResolution()
        {
            // Current mode = ENUM_CURRENT_SETTINGS (-1)
            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            EnumDisplaySettings(null, -1, ref dm);

            return (dm.dmPelsWidth, dm.dmPelsHeight);
        }

        public static List<(int width, int height)> GetSupportedResolutions()
        {
            var modes = new List<(int width, int height)>();
            int modeNum = 0;
            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            while (EnumDisplaySettings(null, modeNum, ref dm))
            {
                modes.Add((dm.dmPelsWidth, dm.dmPelsHeight));
                modeNum++;
            }
            return modes;
        }

        public static List<(int width, int height)> GetSupportedNativeResolutions()
        {
            var allModes = GetSupportedResolutions();
            var native = GetNativeResolution();

            double nativeRatio = (double)native.width / native.height;

            var filtered = new List<(int width, int height)>();
            var seen = new HashSet<string>();

            foreach (var m in allModes)
            {
                double ratio = (double)m.width / m.height;

                // Accept resolutions very close to the same aspect ratio
                if (Math.Abs(ratio - nativeRatio) < 0.0001)
                {
                    string key = $"{m.width}x{m.height}";
                    if (!seen.Contains(key))
                    {
                        filtered.Add(m);
                        seen.Add(key);
                    }
                }
            }

            return filtered;
        }

        public static (int width, int height) GetCurrentResolution()
        {
            const int ENUM_CURRENT_SETTINGS = -1;

            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
            {
                Logger.Info($"Current resolution: {dm.dmPelsWidth}x{dm.dmPelsHeight}");
                return (dm.dmPelsWidth, dm.dmPelsHeight);
            }

            Logger.Error("Failed to get current resolution.");
            return (0, 0); // fallback
        }

        public static void SetResolution(int width, int height)
        {
            Logger.Info($"Attempting to set resolution to {width}x{height}...");

            DEVMODE currentMode = new DEVMODE { dmSize = (short)Marshal.SizeOf(typeof(DEVMODE)) };
            bool hasCurrent = EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref currentMode);
            int currentFrequency = hasCurrent ? currentMode.dmDisplayFrequency : 0;
            int currentOrientation = hasCurrent ? currentMode.dmDisplayOrientation : 0;

            DEVMODE targetMode = new DEVMODE { dmSize = (short)Marshal.SizeOf(typeof(DEVMODE)) };
            bool found = false;
            int modeNum = 0;
            
            DEVMODE bestMatch = new DEVMODE { dmSize = (short)Marshal.SizeOf(typeof(DEVMODE)) };
            int bestFrequency = -1;

            while (EnumDisplaySettings(null, modeNum++, ref targetMode))
            {
                if (targetMode.dmPelsWidth == width && targetMode.dmPelsHeight == height)
                {
                    // If we find a mode with matching frequency, that's perfect.
                    if (targetMode.dmDisplayFrequency == currentFrequency)
                    {
                        bestMatch = targetMode;
                        found = true;
                        break;
                    }
                    
                    // Otherwise, keep track of the one with highest frequency as a fallback.
                    if (targetMode.dmDisplayFrequency > bestFrequency)
                    {
                        bestFrequency = targetMode.dmDisplayFrequency;
                        bestMatch = targetMode;
                        found = true;
                    }
                }
            }

            if (!found)
            {
                Logger.Error($"Could not find a supported mode for {width}x{height}.");
                return;
            }

            // Important: We want to preserve the current orientation.
            // Many handhelds have internal panels that are physically portrait but used as landscape.
            if (hasCurrent)
            {
                bestMatch.dmDisplayOrientation = currentOrientation;
                bestMatch.dmFields |= DM_DISPLAYORIENTATION;
            }

            // Test before applying
            int testResult = ChangeDisplaySettings(ref bestMatch, CDS_TEST);
            if (testResult != DISP_CHANGE_SUCCESSFUL)
            {
                Logger.Warn($"Test failed for mode {width}x{height} @ {bestMatch.dmDisplayFrequency}Hz with orientation {bestMatch.dmDisplayOrientation}. Error: {testResult}. Trying without orientation flag.");
                bestMatch.dmFields &= ~DM_DISPLAYORIENTATION;
                testResult = ChangeDisplaySettings(ref bestMatch, CDS_TEST);
            }

            if (testResult != DISP_CHANGE_SUCCESSFUL)
            {
                Logger.Error($"Failed to validate resolution {width}x{height}. Error code: {testResult}");
                return;
            }

            // Apply permanently
            int result = ChangeDisplaySettings(ref bestMatch, CDS_UPDATEREGISTRY);
            if (result == DISP_CHANGE_SUCCESSFUL)
            {
                Logger.Info($"Resolution changed successfully to {width}x{height}.");
            }
            else
            {
                Logger.Error($"Failed to apply resolution {width}x{height}. Error code: {result}");
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        const uint INPUT_KEYBOARD = 1;
        const uint KEYEVENTF_KEYUP = 0x0002;

        public static void SendKeyCombo(ushort modifier1, ushort modifier2, ushort key)
        {
            INPUT[] inputs = new INPUT[6];

            // Press modifier1 (e.g., Ctrl)
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].u.ki.wVk = modifier1;

            // Press modifier2 (e.g., Shift)
            inputs[1].type = INPUT_KEYBOARD;
            inputs[1].u.ki.wVk = modifier2;

            // Press key (e.g., S)
            inputs[2].type = INPUT_KEYBOARD;
            inputs[2].u.ki.wVk = key;

            //// Release key
            //inputs[3].type = INPUT_KEYBOARD;
            //inputs[3].u.ki.wVk = key;
            //inputs[3].u.ki.dwFlags = KEYEVENTF_KEYUP;

            //// Release modifier2
            //inputs[4].type = INPUT_KEYBOARD;
            //inputs[4].u.ki.wVk = modifier2;
            //inputs[4].u.ki.dwFlags = KEYEVENTF_KEYUP;

            //// Release modifier1
            //inputs[5].type = INPUT_KEYBOARD;
            //inputs[5].u.ki.wVk = modifier1;
            //inputs[5].u.ki.dwFlags = KEYEVENTF_KEYUP;

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public const UInt32 WM_KEYDOWN = 0x0100;
        public const int VK_CONTROL = 0x11;
        public const int VK_SHIFT = 0x10;
        public const int VK_O = 0x4F; // Virtual key code for 'O'
        public const uint WM_CLOSE = 0x0010;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    }
}
