using RTSSSharedMemoryNET;
using System;
using System.Diagnostics;
using XboxGamingBarHelper.Performance;
using XboxGamingBarHelper.RTSS.OSDItems;

namespace XboxGamingBarHelper.RTSS
{
    internal static class RTSSManager
    {
        private const string OSDSeparator = " <C=6E006A>|<C> ";
        private const string OSDAppName = "Xbox Gaming Bar OSD";

        internal static int OSDLevel;
        internal static OSD OSD;
        internal static OSDItem[] OSDItems;

        public static void Initialize()
        {
            OSDItems = new OSDItem[]
            {
                new OSDItem_FPS(),
                new OSDItem_CPU(PerformanceManager.CPUUsage, PerformanceManager.CPUClock, PerformanceManager.CPUWattage, PerformanceManager.CPUTemperature),
            };
        }

        public static bool IsRTSSRunning()
        {
            return Process.GetProcessesByName("RTSS").Length > 0;
        }

        public static void Update()
        {
            Console.WriteLine($"OSD level {OSDLevel}");
            if (OSDLevel == 0)
            {
                if (OSD != null)
                {
                    OSD.Update(string.Empty);
                    OSD.Dispose();
                    OSD = null;
                }

                var osdEntries = OSD.GetOSDEntries();
                for (int i = 0; i < osdEntries.Length; i++)
                {
                    OSDEntry osdEntry = osdEntries[i];
                    if (osdEntry.Owner == OSDAppName)
                    {
                        osdEntry.Text = string.Empty;
                    }
                }

                return;
            }

            if (OSD == null)
            {
                OSD = new OSD(OSDAppName);
                //var osdEntry = new OSDEntry();
                //var appEntry = new AppEntry();
            }

            string osdString = string.Empty;
            for (int i = 0; i < OSDItems.Length; i++)
            {
                var osdItem = OSDItems[i];
                if (i == 0)
                {
                    osdString = osdItem.GetOSDString(OSDLevel);
                }
                else
                {
                    osdString += OSDSeparator + osdItem.GetOSDString(OSDLevel);
                }
            }

            OSD.Update(osdString);
        }
    }
}
