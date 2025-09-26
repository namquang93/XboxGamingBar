using RTSSSharedMemoryNET;
using System.Diagnostics;

namespace XboxGamingBarHelper.RTSS
{
    internal static class RTSSManager
    {
        internal static int OSDLevel;
        internal static OSD OSD;

        public static bool IsRTSSRunning()
        {
            return Process.GetProcessesByName("RTSS").Length > 0;
        }

        public static void Update()
        {
            //if (OSDLevel == 0)
            //{
            //    if (OSD != null)
            //    {
            //        OSD.Update(string.Empty);
            //        OSD.Dispose();
            //        OSD = null;
            //    }

            //    return;
            //}

            if (OSD == null)
            {
                OSD = new OSD("Xbox Gaming Bar OSD");
                //var osdEntry = new OSDEntry();
                //var appEntry = new AppEntry();
            }

            OSD.Update("Xbox Gaming Bar");
        }
    }
}
