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
        }

        public override void Update()
        {
            base.Update();


        }
    }
}
