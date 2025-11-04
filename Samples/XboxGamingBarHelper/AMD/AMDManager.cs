using System;
using Windows.ApplicationModel.AppService;
using XboxGamingBarHelper.AMD.Properties;
using XboxGamingBarHelper.AMD.Settings;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.AMD
{
    internal class AMDManager : Manager
    {
        // ADLX stuff
        private readonly ADLX_RESULT adlxInitializeResult;
        private readonly ADLXHelper adlxHelper;
        private readonly IADLXSystem adlxSystemSevices;
        private readonly IADLXDisplayServices adlxDisplayServices;
        private readonly IADLXGPU adlxInternalGPU;
        private readonly IADLXGPU adlxDedicatedGPU;
        private readonly IADLXGPU adlxSecondDedicatedGPU;
        private readonly IADLX3DSettingsServices2 adlx3DSettingsServices;

        // AMD Settings.
        private readonly AMDRadeonSuperResolutionSetting amdRadeonSuperResolutionSetting;
        public AMDRadeonSuperResolutionSetting AMDRadeonSuperResolutionSetting
        {
            get { return amdRadeonSuperResolutionSetting; }
        }

        // AMD Properties.
        private readonly AMDRadeonSuperResolutionSupportedProperty amdRadeonSuperResolutionSupported;
        public AMDRadeonSuperResolutionSupportedProperty AMDRadeonSuperResolutionSupported
        {
            get { return amdRadeonSuperResolutionSupported; }
        }

        private readonly AMDRadeonSuperResolutionEnabledProperty amdRadeonSuperResolutionEnabled;
        public AMDRadeonSuperResolutionEnabledProperty AMDRadeonSuperResolutionEnabled
        {
            get { return amdRadeonSuperResolutionEnabled; }
        }

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

            Logger.Info("Get AMD display services.");
            // Get display services
            var displayServicesPointer = ADLX.new_displaySerP_Ptr();
            adlxSystemSevices.GetDisplaysServices(displayServicesPointer);
            adlxDisplayServices = ADLX.displaySerP_Ptr_value(displayServicesPointer);

            // Get GPU
            var gpuListPointer = ADLX.new_gpuListP_Ptr();
            adlxSystemSevices.GetGPUs(gpuListPointer);
            var gpuList = ADLX.gpuListP_Ptr_value(gpuListPointer);

            Logger.Info($"Found {gpuList.Size()} GPU.");
            for (uint i = 0; i < gpuList.Size(); i++)
            {
                var gpuPointer = ADLX.new_gpuP_Ptr();
                gpuList.At(i, gpuPointer);
                var gpu = ADLX.gpuP_Ptr_value(gpuPointer);

                var gpuIsExternalPointer = ADLX.new_boolP();
                gpu.IsExternal(gpuIsExternalPointer);
                var gpuIsExternal = ADLX.boolP_value(gpuIsExternalPointer);
                if (gpuIsExternal)
                {
                    if (adlxDedicatedGPU == null)
                    {
                        adlxDedicatedGPU = gpu;
                        Logger.Info("Found a dGPU.");
                    }
                    else if (adlxSecondDedicatedGPU == null)
                    {
                        adlxSecondDedicatedGPU = gpu;
                        Logger.Info("Found second dGPU.");
                    }
                    else
                    {
                        Logger.Warn("Found too many dGPUs.");
                    }
                }
                else
                {
                    if (adlxInternalGPU == null)
                    {
                        adlxInternalGPU = gpu;
                        Logger.Info("Found an iGPU.");
                    }
                    else
                    {
                        Logger.Warn("Found too many iGPUs.");
                    }
                }
            }

            Logger.Info("Get AMD 3D Settings Services.");
            var threeDSettingsServicesPointer = ADLX.new_threeDSettingsSerP_Ptr();
            adlxSystemSevices.Get3DSettingsServices(threeDSettingsServicesPointer);
            adlx3DSettingsServices = new IADLX3DSettingsServices2(ADLXPINVOKE.threeDSettingsSerP_Ptr_value(SWIGTYPE_p_p_adlx__IADLX3DSettingsServices.getCPtr(threeDSettingsServicesPointer)), false);

            Logger.Info("Get Radeon Super Resolution.");
            var threeDRadeonSuperResolutionPointer = ADLX.new_threeDRadeonSuperResolutionP_Ptr();
            adlx3DSettingsServices.GetRadeonSuperResolution(threeDRadeonSuperResolutionPointer);
            var threeDRadeonSuperResolution = ADLX.threeDRadeonSuperResolutionP_Ptr_value(threeDRadeonSuperResolutionPointer);
            amdRadeonSuperResolutionSetting = new AMDRadeonSuperResolutionSetting(threeDRadeonSuperResolution);
            amdRadeonSuperResolutionSupported = new AMDRadeonSuperResolutionSupportedProperty(amdRadeonSuperResolutionSetting.IsSupported(), this);
            amdRadeonSuperResolutionEnabled = new AMDRadeonSuperResolutionEnabledProperty(amdRadeonSuperResolutionSetting.IsEnabled(), this);
        }

        ~AMDManager()
        {
            adlxDisplayServices?.Dispose();
            adlxInternalGPU?.Dispose();
            adlxDedicatedGPU?.Dispose();
            adlxSecondDedicatedGPU?.Dispose();
            adlx3DSettingsServices?.Dispose();
            amdRadeonSuperResolutionSetting?.Dispose();
            adlxHelper?.Dispose();
        }

        public override void Update()
        {
            base.Update();


        }
    }
}
