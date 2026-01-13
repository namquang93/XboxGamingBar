using System;
using System.IO;
using System.Diagnostics;
using RTSSSharedMemoryNET;
using Shared.Enums;
using Shared.Utilities;
using Windows.ApplicationModel.AppService;
using XboxGamingBarHelper.OnScreenDisplay;
using XboxGamingBarHelper.Hardware;
using XboxGamingBarHelper.RTSS.OSDItems;
using System.Collections.Generic;

namespace XboxGamingBarHelper.RTSS
{
    internal class RTSSManager : OnScreenDisplayManager
    {
        // START IOnScreenDisplayProvider implementation
        public override bool IsInstalled => RTSSHelper.IsInstalled(out _);
        // END IOnScreenDisplayProvider implementation

        private const string OSDVerticalLineSeparator = " <C=6E006A>|<C> ";
        private const string OSDNewLine = "\n";
        private const string OSDNewLinePadding = " ";
        private const string OSDSingleLineShortBackground = "<M=0,0,0,0><P=0,0><L0><C=80000000><B=0,0>\b<C>";
        private const string OSDSingleLineFullwidthBackground = "<M=0,0,-3000,0><P=0,0><L0><C=80000000><B=0,0>\b<C>";
        private const string OSDMultipleLinesBackground = "<M=0,0,0,0><P=0,0><L0><C=80000000><B=0,0>\b<C><A0=4><A1=10>";
        private const string OSDAppName = "Gaming Bar OSD";

        private OSD rtssOSD;
        private readonly OSDItem[] osdItems;

        public RTSSManager(HardwareManager hardwareManager, AppServiceConnection connection) : base(connection)
        {
            var osdItemsList = new List<OSDItem>()
            {
                new OSDItemBattery(hardwareManager.BatteryLevel, hardwareManager.BatteryDischargeRate, hardwareManager.BatteryChargeRate, hardwareManager.BatteryRemainingTime),
                new OSDItemGPU(hardwareManager.GPUUsage, hardwareManager.GPUClock, hardwareManager.GPUWattage, hardwareManager.GPUTemperature),
                new OSDItemCPU(hardwareManager.CPUUsage, hardwareManager.CPUClock, hardwareManager.CPUWattage, hardwareManager.CPUTemperature),
            };

            for (int i = 0; i < hardwareManager.CPUCoreUsages.Length; i++)
            {
                osdItemsList.Add(new OSDItemCPUPerCore(i, hardwareManager.CPUCoreUsages[i], hardwareManager.CPUCoreClocks[i]));
            }

            osdItemsList.Add(new OSDItemVideoMemory(hardwareManager.GPUMemoryUsed, hardwareManager.GPUMemoryTotal));
            osdItemsList.Add(new OSDItemMemory(hardwareManager.MemoryUsage, hardwareManager.MemoryUsed));
            osdItemsList.Add(new OSDItemFPS());
            osdItemsList.Add(new OSDItemFramtimeGraph());

            osdItems = osdItemsList.ToArray();
        }

        public override void Update()
        {
            base.Update();

            if (!RTSSHelper.IsInstalled(out string installDir))
            {
                Logger.Debug("Rivatuner Statistics Server is not installed.");
                return;
            }

            var isRunning = RTSSHelper.IsRunning();
            string executablePath = Path.Combine(installDir, $"{RTSSHelper.RTSS_FILE_NAME}.exe");
            if (!isRunning && !File.Exists(executablePath))
            {
                Logger.Debug("Rivatuner Statistics Server is installed but the exe file is not found.");
                applicationState = ApplicationState.NotInstalled;
                return;
            }

            if (onScreenDisplayLevel == 0)
            {
                if (rtssOSD != null)
                {
                    rtssOSD.Update(string.Empty);
                    rtssOSD.Dispose();
                    rtssOSD = null;
                }

                /*var rtssProcess = RTSSHelper.GetProcess();
                if (rtssProcess != null && SettingsManager.GetInstance().AutoStartRTSS)
                {
                    try
                    {
                        Logger.Info("Stopping Rivatuner Statistics Server..");
                        rtssProcess.Kill();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Failed to stop Rivatuner Statistics Server.");
                    }
                }
                rtssState = RivatunerStatisticsServerState.NotRunning;*/

                return;
            }

            if (!isRunning)
            {
#if !STORE
                if (applicationState == ApplicationState.Starting)
                {
                    Logger.Info("Starting Rivatuner Statistics Server..");
                }
                else
                {
                    applicationState = ApplicationState.Starting;
                    try
                    {
                        Logger.Info("Start Rivatuner Statistics Server.");
                        Process.Start(executablePath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Failed to start Rivatuner Statistics Server.");
                        applicationState = ApplicationState.NotRunning;
                    }
                }
#endif
                return;
            }

            applicationState = ApplicationState.Running;

            if (rtssOSD == null)
            {
                rtssOSD = new OSD(OSDAppName);
            }

            var osdString = onScreenDisplayLevel == 1 ? OSDSingleLineShortBackground : (onScreenDisplayLevel >= 3 ? OSDMultipleLinesBackground : OSDSingleLineFullwidthBackground);
            var needSeparator = false;
            var osdPadding = onScreenDisplayLevel >= 3 ? OSDNewLinePadding : string.Empty;
            var osdSeparator = onScreenDisplayLevel >= 3 ? OSDNewLine : OSDVerticalLineSeparator;
            for (int i = 0; i < osdItems.Length; i++)
            {
                var osdItemString = osdItems[i].GetOSDString(onScreenDisplayLevel);
                if (string.IsNullOrEmpty(osdItemString))
                    continue;

                if (needSeparator)
                {
                    osdString += osdSeparator;
                }

                osdString += osdPadding + osdItemString;
                //Logger.Info("OSD Item: " + osdItemString + " => OSD String: " + osdString);
                needSeparator = true;
            }

            rtssOSD.Update(osdString);
        }
    }
}
