using RTSSSharedMemoryNET;
using System.Diagnostics;
using XboxGamingBarHelper.Core;
using XboxGamingBarHelper.Performance;
using XboxGamingBarHelper.RTSS.OSDItems;

namespace XboxGamingBarHelper.RTSS
{
    internal class RTSSManager : Manager
    {
        private const string OSDSeparator = " <C=6E006A>|<C> ";
        private const string OSDBackground = "<P=0,0><L0><C=80000000><B=0,0>\b<C>";
        private const string OSDAppName = "Xbox Gaming Bar OSD";

        internal int osdLevel;
        internal OSD osd;
        internal OSDItem[] osdItems;

        public RTSSManager(PerformanceManager performanceManager)
        {
            osdItems = new OSDItem[]
            {
                new OSDItemFPS(),
                new OSDItemBattery(performanceManager.BatteryPercent, performanceManager.BatteryRemainTime),
                new OSDItemMemory(performanceManager.MemoryUsage, performanceManager.MemoryUsed),
                new OSDItemCPU(performanceManager.CPUUsage, performanceManager.CPUClock, performanceManager.CPUWattage, performanceManager.CPUTemperature),
                new OSDItemGPU(performanceManager.GPUUsage, performanceManager.GPUClock, performanceManager.GPUWattage, performanceManager.GPUTemperature),
            };
        }

        internal static bool IsRTSSRunning()
        {
            return Process.GetProcessesByName("RTSS").Length > 0;
        }

        public override void Update()
        {
            base.Update();
            // Console.WriteLine($"OSD level {OSDLevel}");
            
            if (osdLevel == 0)
            {
                if (osd != null)
                {
                    osd.Update(string.Empty);
                    osd.Dispose();
                    osd = null;
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

            if (osd == null)
            {
                osd = new OSD(OSDAppName);
            }

            string osdString = OSDBackground;
            for (int i = 0; i < osdItems.Length; i++)
            {
                var osdItemString = osdItems[i].GetOSDString(osdLevel);
                if (string.IsNullOrEmpty(osdItemString))
                    continue;

                if (i == 0)
                {
                    osdString += osdItemString;
                }
                else
                {
                    osdString += OSDSeparator + osdItemString;
                }
            }

            osd.Update(osdString);
        }
    }
}
