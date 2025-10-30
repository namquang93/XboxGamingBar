using System;
using Windows.ApplicationModel.AppService;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.AMD
{
    internal class AMDManager : Manager
    {
        private ADLXHelper adlxHelper;
        private IADLXSystem adlxSystemSevices;
        private ADLX_RESULT adlxInitializeResult;

        public AMDManager(AppServiceConnection connection) : base(connection)
        {
            // Initialize ADLX with ADLXHelper
            adlxHelper = new ADLXHelper();
            adlxInitializeResult = adlxHelper.Initialize();

            if (adlxInitializeResult != ADLX_RESULT.ADLX_OK)
            {
                Logger.Error("AMD Manager initialize failed.");
                return;
            }

            adlxSystemSevices = adlxHelper.GetSystemServices();
            if (adlxSystemSevices == null)
            {
                Logger.Error("Can't get AMD system service.");
                return;
            }
            // Get display services
            SWIGTYPE_p_p_adlx__IADLXDisplayServices s = ADLX.new_displaySerP_Ptr();
            var getDisplayServiceResult = adlxSystemSevices.GetDisplaysServices(s);
            //adlxSystemSevices.GetPerformanceMonitoringServices()
            global::System.IntPtr cPtr = ADLXPINVOKE.new_displaySerP_Ptr();
            SWIGTYPE_p_p_adlx__IADLX3DSettingsServices s2 = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_p_adlx__IADLX3DSettingsServices(cPtr, false);
            var threeDResult = adlxSystemSevices.Get3DSettingsServices(s2);
            Logger.Info($"3D: {threeDResult}");
            IADLXDisplayServices displayService = ADLX.displaySerP_Ptr_value(s);

            if (getDisplayServiceResult != ADLX_RESULT.ADLX_OK)
            {
                Logger.Error("Can't get AMD display service.");
                return;
            }

            SWIGTYPE_p_p_adlx__IADLXDisplayList ppDisplayList = ADLX.new_displayListP_Ptr();
            var getDisplayListResult = displayService.GetDisplays(ppDisplayList);
            IADLXDisplayList displayList = ADLX.displayListP_Ptr_value(ppDisplayList);

            if (getDisplayListResult == ADLX_RESULT.ADLX_OK)
            {
                // Iterate through the display list
                uint it = displayList.Begin();
                for (; it != displayList.Size(); it++)
                {
                    SWIGTYPE_p_p_adlx__IADLXDisplay ppDisplay = ADLX.new_displayP_Ptr();
                    var getDisplayResult = displayList.At(it, ppDisplay);
                    IADLXDisplay display = ADLX.displayP_Ptr_value(ppDisplay);

                    if (getDisplayResult == ADLX_RESULT.ADLX_OK)
                    {
                        SWIGTYPE_p_p_char ppName = ADLX.new_charP_Ptr();
                        display.Name(ppName);
                        String name = ADLX.charP_Ptr_value(ppName);

                        SWIGTYPE_p_ADLX_DISPLAY_TYPE pDisType = ADLX.new_displayTypeP();
                        display.DisplayType(pDisType);
                        ADLX_DISPLAY_TYPE disType = ADLX.displayTypeP_value(pDisType);

                        SWIGTYPE_p_unsigned_int pMID = ADLX.new_uintP();
                        display.ManufacturerID(pMID);
                        long mid = ADLX.uintP_value(pMID);

                        SWIGTYPE_p_ADLX_DISPLAY_CONNECTOR_TYPE pConnect = ADLX.new_disConnectTypeP();
                        display.ConnectorType(pConnect);
                        ADLX_DISPLAY_CONNECTOR_TYPE connect = ADLX.disConnectTypeP_value(pConnect);

                        SWIGTYPE_p_p_char ppEDIE = ADLX.new_charP_Ptr();
                        display.EDID(ppEDIE);
                        String edid = ADLX.charP_Ptr_value(ppEDIE);

                        SWIGTYPE_p_int pH = ADLX.new_intP();
                        SWIGTYPE_p_int pV = ADLX.new_intP();
                        display.NativeResolution(pH, pV);
                        int h = ADLX.intP_value(pH);
                        int v = ADLX.intP_value(pV);

                        SWIGTYPE_p_double pRefRate = ADLX.new_doubleP();
                        display.RefreshRate(pRefRate);
                        double refRate = ADLX.doubleP_value(pRefRate);

                        SWIGTYPE_p_unsigned_int pPixClock = ADLX.new_uintP();
                        display.PixelClock(pPixClock);
                        long pixClock = ADLX.uintP_value(pPixClock);

                        SWIGTYPE_p_ADLX_DISPLAY_SCAN_TYPE pScanType = ADLX.new_disScanTypeP();
                        display.ScanType(pScanType);
                        ADLX_DISPLAY_SCAN_TYPE scanType = ADLX.disScanTypeP_value(pScanType);

                        SWIGTYPE_p_size_t pID = ADLX.new_adlx_sizeP();
                        display.UniqueId(pID);
                        uint id = ADLX.adlx_sizeP_value(pID);

                        Logger.Info(String.Format("\nThe display [{0}]:", it));
                        Logger.Info(String.Format("\tName: {0}", name));
                        Logger.Info(String.Format("\tType: {0}", disType));
                        Logger.Info(String.Format("\tConnector type: {0}", connect));
                        Logger.Info(String.Format("\tManufacturer id: {0}", mid));
                        Logger.Info(String.Format("\tEDID: {0}", edid));
                        Logger.Info(String.Format("\tResolution:  h: {0}  v: {1}", h, v));
                        Logger.Info(String.Format("\tRefresh rate: {0}", refRate));
                        Logger.Info(String.Format("\tPixel clock: {0}", pixClock));
                        Logger.Info(String.Format("\tScan type: {0}", scanType));
                        Logger.Info(String.Format("\tUnique id: {0}", id));

                        // Release display interface
                        display.Release();
                    }
                }

                // Release display list interface
                displayList.Release();
            }

            SWIGTYPE_p_p_adlx__IADLXPerformanceMonitoringServices performanceMonitoringServicesPointer = ADLX.new_performanceMonitoringSerP_Ptr();
            var res = adlxSystemSevices.GetPerformanceMonitoringServices(performanceMonitoringServicesPointer);
            SWIGTYPE_p_p_adlx__IADLX3DSettingsServices threeDSettingsServicesPointer = ADLX.new_threeDSettingsSerP_Ptr();
            adlxSystemSevices.Get3DSettingsServices(threeDSettingsServicesPointer);
            IADLX3DSettingsServices threeDSettingsServices = ADLX.threeDSettingsSerP_Ptr_value(threeDSettingsServicesPointer);
            if (res == ADLX_RESULT.ADLX_OK)
            {
                IADLXPerformanceMonitoringServices performanceMonitoringServices = ADLX.performanceMonitoringSerP_Ptr_value(performanceMonitoringServicesPointer);
                SWIGTYPE_p_p_adlx__IADLXSystemMetricsSupport systemMetricsSupportPointer = ADLX.new_systemMetricsSupportP_Ptr();
                res = performanceMonitoringServices.GetSupportedSystemMetrics(systemMetricsSupportPointer);
                if (res == ADLX_RESULT.ADLX_OK)
                {
                    IADLXSystemMetricsSupport systemMetricSupport = ADLX.systemMetricsSupportP_Ptr_value(systemMetricsSupportPointer);
                    SWIGTYPE_p_bool pSupportedCPUUsage = ADLX.new_boolP();
                    ADLX_RESULT checkCPUUsageSupportedResult = systemMetricSupport.IsSupportedCPUUsage(pSupportedCPUUsage);
                    if (checkCPUUsageSupportedResult == ADLX_RESULT.ADLX_OK)
                    {
                        bool isSupportedCPUUsage = ADLX.boolP_value(pSupportedCPUUsage);
                        Logger.Info($"{(isSupportedCPUUsage ? "Support" : "Doesn't support")} CPU usage");
                    }
                    else
                    {
                        Logger.Info("Can't determine CPU usage support");
                    }
                    systemMetricSupport.Release();
                }
                else
                {
                    Logger.Info("Can't get supported system metrics");
                }

                SWIGTYPE_p_p_adlx__IADLXFPSList fpsListPointer = ADLX.new_fpsListP_Ptr();
                res = performanceMonitoringServices.GetFPSHistory(0, 5, fpsListPointer);
                if (res == ADLX_RESULT.ADLX_OK)
                {
                    IADLXFPSList fpsList = ADLX.fpsListP_Ptr_value(fpsListPointer);
                    Logger.Info($"Got FPS list {fpsList.Size()}");
                    fpsList.Release();
                }
                else
                {
                    Logger.Info("Can't get FPS list");
                }

                SWIGTYPE_p_int performanceMetricsHistorySizePointer = ADLX.new_intP();
                performanceMonitoringServices.GetCurrentPerformanceMetricsHistorySize(performanceMetricsHistorySizePointer);
                int performanceMetricsHistorySize = ADLX.intP_value(performanceMetricsHistorySizePointer);
                SWIGTYPE_p_p_adlx__IADLXAllMetricsList allMetricsListPointer = ADLX.new_allMetricsListP_Ptr();
                res = performanceMonitoringServices.GetAllMetricsHistory(0, 5, allMetricsListPointer);
                if (res == ADLX_RESULT.ADLX_OK)
                {
                    IADLXAllMetricsList allMetricsList = ADLX.allMetricsListP_Ptr_value(allMetricsListPointer);
                    Logger.Info($"All metrics list Size={allMetricsList.Size()}");
                    allMetricsList.Release();
                    //allMetricsList.QueryInterface()
                }
                else
                {
                    Logger.Info($"Can't get all metrics list, PerformanceMetricsHistorySize={performanceMetricsHistorySize}");
                }

                SWIGTYPE_p_p_adlx__IADLXAllMetrics allMetricsPointer = ADLX.new_allMetricsP_Ptr();
                performanceMonitoringServices.GetCurrentAllMetrics(allMetricsPointer);
                IADLXAllMetrics allMetrics = ADLX.allMetricsP_Ptr_value(allMetricsPointer);

                SWIGTYPE_p_long_long timestampPointer = ADLX.new_int64P();
                allMetrics.TimeStamp(timestampPointer);
                long timeStamp = ADLX.int64P_value(timestampPointer);

                SWIGTYPE_p_p_adlx__IADLXFPS adlxFPSPointer = ADLX.new_fpsP_Ptr();
                allMetrics.GetFPS(adlxFPSPointer);
                IADLXFPS adlxFPS = ADLX.fpsP_Ptr_value(adlxFPSPointer);
                SWIGTYPE_p_int fpsPointer = ADLX.new_intP();
                adlxFPS.FPS(fpsPointer);
                int fps = ADLX.intP_value(fpsPointer);

                SWIGTYPE_p_p_adlx__IADLXSystemMetrics systemMetricsPointer = ADLX.new_systemMetricsP_Ptr();
                allMetrics.GetSystemMetrics(systemMetricsPointer);
                IADLXSystemMetrics systemMetrics = ADLX.systemMetricsP_Ptr_value(systemMetricsPointer);

                SWIGTYPE_p_double cpuUsagePointer = ADLX.new_doubleP();
                systemMetrics.CPUUsage(cpuUsagePointer);
                double cpuUsage = ADLX.doubleP_value(cpuUsagePointer);

                SWIGTYPE_p_int smartShiftPointer = ADLX.new_intP();
                systemMetrics.SmartShift(smartShiftPointer);
                int smartShift = ADLX.intP_value(smartShiftPointer);

                SWIGTYPE_p_int systemRAMPointer = ADLX.new_intP();
                systemMetrics.SystemRAM(systemRAMPointer);
                int systemRAM = ADLX.intP_value(systemRAMPointer);

                Logger.Info($"[{timeStamp}] FPS={fps} CPUUsage={cpuUsage} SmartShift={smartShift} RAM={systemRAM}");

                SWIGTYPE_p_p_adlx__IADLXGPUList gpuListPointer = ADLX.new_gpuListP_Ptr();
                adlxSystemSevices.GetGPUs(gpuListPointer);
                IADLXGPUList gpuList = ADLX.gpuListP_Ptr_value(gpuListPointer);
                for (uint i = 0; i < gpuList.Size(); i++)
                {
                    SWIGTYPE_p_p_adlx__IADLXGPU gpuPointer = ADLX.new_gpuP_Ptr();
                    gpuList.At(i, gpuPointer);
                    IADLXGPU gpu = ADLX.gpuP_Ptr_value(gpuPointer);

                    SWIGTYPE_p_p_adlx__IADLXGPUMetrics gpuMetricsPointer = ADLX.new_gpuMetricsP_Ptr();
                    allMetrics.GetGPUMetrics(gpu, gpuMetricsPointer);
                    IADLXGPUMetrics gpuMetrics = ADLX.gpuMetricsP_Ptr_value(gpuMetricsPointer);

                    SWIGTYPE_p_long_long gpuTimestampPointer = ADLX.new_int64P();
                    gpuMetrics.TimeStamp(gpuTimestampPointer);
                    long gpuTimeStamp = ADLX.int64P_value(gpuTimestampPointer);

                    SWIGTYPE_p_int gpuClockSpeedPointer = ADLX.new_intP();
                    gpuMetrics.GPUClockSpeed(gpuClockSpeedPointer);
                    int gpuClockSpeed = ADLX.intP_value(gpuClockSpeedPointer);

                    SWIGTYPE_p_int gpuFanSpeedPointer = ADLX.new_intP();
                    gpuMetrics.GPUFanSpeed(gpuFanSpeedPointer);
                    int gpuFanSpeed = ADLX.intP_value(gpuFanSpeedPointer);

                    SWIGTYPE_p_double gpuHotspotTemperaturePointer = ADLX.new_doubleP();
                    gpuMetrics.GPUHotspotTemperature(gpuHotspotTemperaturePointer);
                    double gpuHotspotTemperature = ADLX.doubleP_value(gpuHotspotTemperaturePointer);

                    SWIGTYPE_p_double gpuIntakeTemperaturePointer = ADLX.new_doubleP();
                    gpuMetrics.GPUIntakeTemperature(gpuIntakeTemperaturePointer);
                    double gpuIntakeTemperature = ADLX.doubleP_value(gpuIntakeTemperaturePointer);

                    SWIGTYPE_p_double gpuPowerPointer = ADLX.new_doubleP();
                    gpuMetrics.GPUPower(gpuPowerPointer);
                    double gpuPower = ADLX.doubleP_value(gpuPowerPointer);

                    SWIGTYPE_p_double gpuTemperaturePointer = ADLX.new_doubleP();
                    gpuMetrics.GPUTemperature(gpuTemperaturePointer);
                    double gpuTemperature = ADLX.doubleP_value(gpuTemperaturePointer);

                    SWIGTYPE_p_double gpuTotalBoardPowerPointer = ADLX.new_doubleP();
                    gpuMetrics.GPUTotalBoardPower(gpuTotalBoardPowerPointer);
                    double gpuTotalBoardPower = ADLX.doubleP_value(gpuTotalBoardPowerPointer);

                    SWIGTYPE_p_double gpuUsagePointer = ADLX.new_doubleP();
                    gpuMetrics.GPUUsage(gpuUsagePointer);
                    double gpuUsage = ADLX.doubleP_value(gpuUsagePointer);

                    SWIGTYPE_p_int gpuVoltagePointer = ADLX.new_intP();
                    gpuMetrics.GPUVoltage(gpuVoltagePointer);
                    int gpuVoltage = ADLX.intP_value(gpuVoltagePointer);

                    SWIGTYPE_p_int gpuVRAMPointer = ADLX.new_intP();
                    gpuMetrics.GPUVRAM(gpuVRAMPointer);
                    int gpuVRAM = ADLX.intP_value(gpuVRAMPointer);

                    SWIGTYPE_p_int gpuVRAMClockSpeedPointer = ADLX.new_intP();
                    gpuMetrics.GPUVRAMClockSpeed(gpuVRAMClockSpeedPointer);
                    int gpuVRAMClockSpeed = ADLX.intP_value(gpuVRAMClockSpeedPointer);

                    SWIGTYPE_p_p_adlx__IADLXGPUMetricsList gpuMetricsHistoryPointer = ADLX.new_gpuMetricsListP_Ptr();
                    res = performanceMonitoringServices.GetGPUMetricsHistory(gpu, 0, 5, gpuMetricsHistoryPointer);
                    if (res == ADLX_RESULT.ADLX_OK)
                    {
                        IADLXGPUMetricsList gpuMetricsHistory = ADLX.gpuMetricsListP_Ptr_value(gpuMetricsHistoryPointer);
                        Logger.Info($"GPU metric history {gpuMetricsHistory.Size()}");
                    }
                    else
                    {
                        Logger.Info("Can't get GPU metric history");
                    }

                    Logger.Info($"[{gpuTimeStamp}] GPU {i} ClockSpeed={gpuClockSpeed} FanSpeed={gpuFanSpeed} HotspotTemperature={gpuHotspotTemperature} IntakeTemperature={gpuIntakeTemperature} Power={gpuPower} Temperature={gpuTemperature} TotalBoardPower={gpuTotalBoardPower} Usage={gpuUsage} Voltage={gpuVoltage} VRAM={gpuVRAM} VRAMClockSpeed={gpuVRAMClockSpeed}");
                    gpuMetrics.Release();

                    SWIGTYPE_p_p_adlx__IADLX3DSettingsChangedHandling threeDSettingsChangedHandlingPointer = ADLX.new_threeDSettingsChangedHandlingP_Ptr();
                    threeDSettingsServices.Get3DSettingsChangedHandling(threeDSettingsChangedHandlingPointer);
                    IADLX3DSettingsChangedHandling threeDSettingsChangedHandling = ADLX.threeDSettingsChangedHandlingP_Ptr_value(threeDSettingsChangedHandlingPointer);
                    //IADLX3DSettingsChangedListener threeDSettingsChangedListener = new IADLX3DSettingsChangedListener();
                    //threeDSettingsChangedHandling.Add3DSettingsEventListener(threeDSettingsChangedListener);
                    Logger.Info("Got 3DSettingsChangedHandling");
                    threeDSettingsChangedHandling.Release();

                    SWIGTYPE_p_p_adlx__IADLX3DAnisotropicFiltering threeDAnisotropicFilteringPointer = ADLX.new_threeDAnisotropicFilteringP_Ptr();
                    threeDSettingsServices.GetAnisotropicFiltering(gpu, threeDAnisotropicFilteringPointer);
                    IADLX3DAnisotropicFiltering threeDAnisotropicFiltering = ADLX.threeDAnisotropicFilteringP_Ptr_value(threeDAnisotropicFilteringPointer);
                    SWIGTYPE_p_bool threeDAnisotropicFilteringIsSupportedPointer = ADLX.new_boolP();
                    threeDAnisotropicFiltering.IsSupported(threeDAnisotropicFilteringIsSupportedPointer);
                    bool threeDAnisotropicFilteringIsSupported = ADLX.boolP_value(threeDAnisotropicFilteringIsSupportedPointer);
                    SWIGTYPE_p_bool threeDAnisotropicFilteringIsEnabledPointer = ADLX.new_boolP();
                    threeDAnisotropicFiltering.IsEnabled(threeDAnisotropicFilteringIsEnabledPointer);
                    bool threeDAnisotropicFilteringIsEnabled = ADLX.boolP_value(threeDAnisotropicFilteringIsEnabledPointer);
                    SWIGTYPE_p_ADLX_ANISOTROPIC_FILTERING_LEVEL anisotropicFilteringLevelPointer = ADLX.new_anisotropicFilteringLevelP();
                    threeDAnisotropicFiltering.GetLevel(anisotropicFilteringLevelPointer);
                    ADLX_ANISOTROPIC_FILTERING_LEVEL anisotropicFilteringLevel = ADLX.anisotropicFilteringLevelP_value(anisotropicFilteringLevelPointer);
                    threeDAnisotropicFiltering.SetEnabled(false);
                    //threeDAnisotropicFiltering.SetLevel(ADLX_ANISOTROPIC_FILTERING_LEVEL.AF_LEVEL_X2);
                    Logger.Info($"AnisotropicFiltering Supported={threeDAnisotropicFilteringIsSupported} IsEnabled={threeDAnisotropicFilteringIsEnabled} Level={anisotropicFilteringLevel}");
                    threeDAnisotropicFiltering.Release();

                    SWIGTYPE_p_p_adlx__IADLX3DAntiAliasing threeDAntiAliasingPointer = ADLX.new_threeDAntiAliasingP_Ptr();
                    threeDSettingsServices.GetAntiAliasing(gpu, threeDAntiAliasingPointer);
                    IADLX3DAntiAliasing threeDAntiAliasing = ADLX.threeDAntiAliasingP_Ptr_value(threeDAntiAliasingPointer);
                    SWIGTYPE_p_bool threeDAntiAliasingSupportedPointer = ADLX.new_boolP();
                    threeDAntiAliasing.IsSupported(threeDAntiAliasingSupportedPointer);
                    bool threeDAntiAliasingSupported = ADLX.boolP_value(threeDAntiAliasingSupportedPointer);
                    SWIGTYPE_p_ADLX_ANTI_ALIASING_LEVEL antiAliasingLevelPointer = ADLX.new_antiAliasingLevelP();
                    threeDAntiAliasing.GetLevel(antiAliasingLevelPointer);
                    ADLX_ANTI_ALIASING_LEVEL antiAliasingLevel = ADLX.antiAliasingLevelP_value(antiAliasingLevelPointer);
                    var antiAliasingMethodPointer = ADLX.new_antiAliasingMethodP();
                    threeDAntiAliasing.GetMethod(antiAliasingMethodPointer);
                    var antiAliasingMethod = ADLX.antiAliasingMethodP_value(antiAliasingMethodPointer);
                    var antiAliasingModePointer = ADLX.new_antiAliasingModeP();
                    threeDAntiAliasing.GetMode(antiAliasingModePointer);
                    var antiAliasingMode = ADLX.antiAliasingModeP_value(antiAliasingModePointer);
                    Logger.Info($"AntiAliasing Supported={threeDAntiAliasingSupported} Level={antiAliasingLevel} Method={antiAliasingMethod} Mode={antiAliasingMode}");
                    threeDAntiAliasing.Release();

                    gpu.Release();
                }

                ADLX_IntRange maxPerformanceMetricsHistorySizeRangePointer = ADLX.new_intRangeP();
                res = performanceMonitoringServices.GetMaxPerformanceMetricsHistorySizeRange(maxPerformanceMetricsHistorySizeRangePointer);
                if (res == ADLX_RESULT.ADLX_OK)
                {
                    ADLX_IntRange maxPerformanceMetricsHistorySizeRange = ADLX.intRangeP_value(maxPerformanceMetricsHistorySizeRangePointer);
                    Logger.Info($"Got max performance metrics history size range {maxPerformanceMetricsHistorySizeRange.minValue} - {maxPerformanceMetricsHistorySizeRange.maxValue}");
                }
                else
                {
                    Logger.Info("Can't get max performance metrics history size range");
                }

                allMetrics.Release();
                systemMetrics.Release();
                adlxFPS.Release();
                gpuList.Release();
                performanceMonitoringServices.Release();
            }
            else
            {
                Logger.Info("Can't get performance monitoring services");
            }

            threeDSettingsServices.Release();
        }

        public override void Update()
        {
            base.Update();


        }
    }
}
