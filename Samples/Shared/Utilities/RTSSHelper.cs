using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace Shared.Utilities
{
    public static partial class RTSSHelper
    {
        public const string RTSS_FILE_NAME = "RTSS";

        public static Process GetProcess()
        {
            var rtssProcessses = Process.GetProcessesByName(RTSS_FILE_NAME);
            if (rtssProcessses.Length == 0)
            {
                return null;
            }

            return rtssProcessses[0];
        }

        public static bool IsRunning()
        {
            var rtssProcess = GetProcess();
            if (rtssProcess == null)
            {
                return false;
            }

            var runningTime = (DateTime.Now - rtssProcess.StartTime).TotalSeconds;
            return runningTime < 0 || runningTime >= 2.0f;
        }

        public static bool IsInstalled(out string installDir)
        {
            installDir = RegistryHelper.ReadStringValue(Registry.LocalMachine, @"Software\WOW6432Node\Unwinder\RTSS", "InstallDir");
            return !string.IsNullOrEmpty(installDir);
        }
    }
}
