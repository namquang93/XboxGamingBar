using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace Shared.Utilities
{
    public static partial class RTSSHelper
    {
        public static bool IsRunning()
        {
            var rtssProcessses = Process.GetProcessesByName("RTSS");
            if (rtssProcessses.Length == 0)
            {
                return false;
            }

            var rtssProcess = rtssProcessses[0];
            return (DateTime.Now - rtssProcess.StartTime).TotalSeconds >= 2.0f;
        }

        public static bool IsInstalled()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\WOW6432Node\Unwinder\RTSS"))
            {
                return key != null;
            }
        }

        public static string InstalledLocation()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\WOW6432Node\Unwinder\RTSS"))
            {
                if (key == null)
                {
                    return string.Empty;
                }

                return (string)key.GetValue("InstallDir");
            }
        }
    }
}
