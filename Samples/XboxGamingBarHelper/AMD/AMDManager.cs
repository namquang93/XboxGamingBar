using Microsoft.Win32;
using System;
using System.Diagnostics;
//using XboxGamingBarHelper.Windows;
using System.Windows.Forms;
using Windows.ApplicationModel.AppService;
using XboxGamingBarHelper.AMD.Properties;
using XboxGamingBarHelper.AMD.Settings;
using XboxGamingBarHelper.OnScreenDisplay;
using XboxGamingBarHelper.Settings;
using XboxGamingBarHelper.Windows;

namespace XboxGamingBarHelper.AMD
{
    internal class AMDManager : OnScreenDisplayManager
    {
        // AMD Software stuff
        // Computer\HKEY_CURRENT_USER\Software\AMD\CN\Performance
        private static readonly RegistryKey AMD_PERFORMANCE_KEY_ROOT = Registry.CurrentUser;
        private const string AMD_PERFORMANCE_KEY_PATH = @"Software\AMD\CN\Performance";
        private const string AMD_PERFORMANCE_STATE_KEY_NAME = "MetricsOverlayState";
        private const string AMD_PERFORMANCE_PROFILE_KEY_NAME = "MetricsProfile";

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

        private readonly AMDFluidMotionFrameSetting amdFluidMotionFrameSetting;
        public AMDFluidMotionFrameSetting AMDFluidMotionFrameSetting
        {
            get { return amdFluidMotionFrameSetting; }
        }

        private readonly AMDRadeonAntiLagSetting amdRadeonAntiLagSetting;
        public AMDRadeonAntiLagSetting AMDRadeonAntiLagSetting
        {
            get { return amdRadeonAntiLagSetting; }
        }

        private readonly AMDRadeonBoostSetting amdRadeonBoostSetting;
        public AMDRadeonBoostSetting AMDRadeonBoostSetting
        {
            get { return amdRadeonBoostSetting; }
        }

        private readonly AMDRadeonChillSetting amdRadeonChillSetting;
        public AMDRadeonChillSetting AMDRadeonChillSetting
        {
            get { return amdRadeonChillSetting; }
        }

        private readonly AMD3DSettingsChangedListener amd3DSettingsChangedListener;

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

        private readonly AMDRadeonSuperResolutionSharpnessProperty amdRadeonSuperResolutionSharpness;
        public AMDRadeonSuperResolutionSharpnessProperty AMDRadeonSuperResolutionSharpness
        {
            get { return amdRadeonSuperResolutionSharpness; }
        }

        private readonly AMDFluidMotionFrameSupportedProperty amdFluidMotionFrameSupported;
        public AMDFluidMotionFrameSupportedProperty AMDFluidMotionFrameSupported
        {
            get { return amdFluidMotionFrameSupported; }
        }

        private readonly AMDFluidMotionFrameEnabledProperty amdFluidMotionFrameEnabled;
        public AMDFluidMotionFrameEnabledProperty AMDFluidMotionFrameEnabled
        {
            get { return amdFluidMotionFrameEnabled; }
        }

        private readonly AMDRadeonAntiLagSupportedProperty amdRadeonAntiLagSupported;
        public AMDRadeonAntiLagSupportedProperty AMDRadeonAntiLagSupported
        {
            get { return amdRadeonAntiLagSupported; }
        }

        private readonly AMDRadeonAntiLagEnabledProperty amdRadeonAntiLagEnabled;
        public AMDRadeonAntiLagEnabledProperty AMDRadeonAntiLagEnabled
        {
            get { return amdRadeonAntiLagEnabled; }
        }

        private readonly AMDRadeonBoostSupportedProperty amdRadeonBoostSupported;
        public AMDRadeonBoostSupportedProperty AMDRadeonBoostSupported
        {
            get { return amdRadeonBoostSupported; }
        }

        private readonly AMDRadeonBoostEnabledProperty amdRadeonBoostEnabled;
        public AMDRadeonBoostEnabledProperty AMDRadeonBoostEnabled
        {
            get { return amdRadeonBoostEnabled; }
        }

        private readonly AMDRadeonBoostResolutionProperty amdRadeonBoostResolution;
        public AMDRadeonBoostResolutionProperty AMDRadeonBoostResolution
        {
            get { return amdRadeonBoostResolution; }
        }

        private readonly AMDRadeonChillSupportedProperty amdRadeonChillSupported;
        public AMDRadeonChillSupportedProperty AMDRadeonChillSupported
        {
            get { return amdRadeonChillSupported; }
        }

        private readonly AMDRadeonChillEnabledProperty amdRadeonChillEnabled;
        public AMDRadeonChillEnabledProperty AMDRadeonChillEnabled
        {
            get { return amdRadeonChillEnabled; }
        }

        private readonly AMDRadeonChillMinFPSProperty amdRadeonChillMinFPS;
        public AMDRadeonChillMinFPSProperty AMDRadeonChillMinFPS
        {
            get { return amdRadeonChillMinFPS; }
        }

        private readonly AMDRadeonChillMaxFPSProperty amdRadeonChillMaxFPS;
        public AMDRadeonChillMaxFPSProperty AMDRadeonChillMaxFPS
        {
            get { return amdRadeonChillMaxFPS; }
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
            amdRadeonSuperResolutionSharpness = new AMDRadeonSuperResolutionSharpnessProperty(amdRadeonSuperResolutionSetting.GetSharpness(), this);

            Logger.Info("Get AMD Fluid Motion Frame.");
            var threeDFluidMotionFramePointer = ADLX.new_threeDAMDFluidMotionFramesP_Ptr();
            adlx3DSettingsServices.GetAMDFluidMotionFrames(threeDFluidMotionFramePointer);
            var threeDFluidMotionFrame = ADLX.threeDAMDFluidMotionFramesP_Ptr_value(threeDFluidMotionFramePointer);
            amdFluidMotionFrameSetting = new AMDFluidMotionFrameSetting(threeDFluidMotionFrame);
            amdFluidMotionFrameSupported = new AMDFluidMotionFrameSupportedProperty(amdFluidMotionFrameSetting.IsSupported(), this);
            amdFluidMotionFrameEnabled = new AMDFluidMotionFrameEnabledProperty(amdFluidMotionFrameSetting.IsEnabled(), this);

            Logger.Info("Get AMD Anti-Lag.");
            var threeDAntiLagPointer = ADLX.new_threeDAntiLagP_Ptr();
            adlx3DSettingsServices.GetAntiLag(adlxInternalGPU, threeDAntiLagPointer);
            var threeDAntiLag = ADLX.threeDAntiLagP_Ptr_value(threeDAntiLagPointer);
            amdRadeonAntiLagSetting = new AMDRadeonAntiLagSetting(threeDAntiLag);
            amdRadeonAntiLagSupported = new AMDRadeonAntiLagSupportedProperty(amdRadeonAntiLagSetting.IsSupported(), this);
            amdRadeonAntiLagEnabled = new AMDRadeonAntiLagEnabledProperty(amdRadeonAntiLagSetting.IsEnabled(), this);

            Logger.Info("Get AMD Radeon Boost.");
            var threeDRadeonBoostPointer = ADLX.new_threeDBoostP_Ptr();
            adlx3DSettingsServices.GetBoost(adlxInternalGPU, threeDRadeonBoostPointer);
            var threeDRadeonBoost = ADLX.threeDBoostP_Ptr_value(threeDRadeonBoostPointer);
            amdRadeonBoostSetting = new AMDRadeonBoostSetting(threeDRadeonBoost);
            amdRadeonBoostSupported = new AMDRadeonBoostSupportedProperty(amdRadeonBoostSetting.IsSupported(), this);
            amdRadeonBoostEnabled = new AMDRadeonBoostEnabledProperty(amdRadeonBoostSetting.IsEnabled(), this);
            var amdRadeonBoostResolutionRange = amdRadeonBoostSetting.GetResolutionRange();
            amdRadeonBoostResolution = new AMDRadeonBoostResolutionProperty(amdRadeonBoostSetting.GetResolution() == amdRadeonBoostResolutionRange.Item1 ? 0 : 1, this);

            Logger.Info("Get AMD Radeon Chill.");
            var threeDRadeonChillPointer = ADLX.new_threeDChillP_Ptr();
            adlx3DSettingsServices.GetChill(adlxInternalGPU, threeDRadeonChillPointer);
            var threeDRadeonChill = ADLX.threeDChillP_Ptr_value(threeDRadeonChillPointer);
            amdRadeonChillSetting = new AMDRadeonChillSetting(threeDRadeonChill);
            amdRadeonChillEnabled = new AMDRadeonChillEnabledProperty(amdRadeonChillSetting.IsEnabled(), this);
            amdRadeonChillSupported = new AMDRadeonChillSupportedProperty(amdRadeonChillSetting.IsSupported(), this);
            amdRadeonChillMinFPS = new AMDRadeonChillMinFPSProperty(amdRadeonChillSetting.GetMinFPS(), this);
            amdRadeonChillMaxFPS = new AMDRadeonChillMaxFPSProperty(amdRadeonChillSetting.GetMaxFPS(), this);

            Logger.Info("AMD Manager initialized successfully.");

            amdFluidMotionFrameEnabled.PropertyChanged += AmdFluidMotionFrameEnabled;
            amdRadeonAntiLagEnabled.PropertyChanged += AmdRadeonAntiLagEnabled;
            amdRadeonBoostEnabled.PropertyChanged += AmdRadeonBoostEnabled;
            amdRadeonChillEnabled.PropertyChanged += AmdRadeonChillEnabled;

            var threeDSettingsChangedHandlingPointer = ADLX.new_threeDSettingsChangedHandlingP_Ptr();
            //ADLX.new_threeDSettingsChangedHandlingP_Ptr
            adlx3DSettingsServices.Get3DSettingsChangedHandling(threeDSettingsChangedHandlingPointer);
            var threeDSettingsChangedHandling = ADLX.threeDSettingsChangedHandlingP_Ptr_value(threeDSettingsChangedHandlingPointer);
            amd3DSettingsChangedListener = new AMD3DSettingsChangedListener(this);
            threeDSettingsChangedHandling.Add3DSettingsEventListener(amd3DSettingsChangedListener);
        }

        private void AmdRadeonChillEnabled(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (amdRadeonChillEnabled)
            {
                if (amdRadeonAntiLagSupported && amdRadeonAntiLagEnabled)
                {
                    Logger.Info($"AMD Radeon Chill enabled, Radeon Anti-Lag should be disabled too.");
                    amdRadeonAntiLagEnabled.SetValue(false);
                }
                else
                {
                    Logger.Info($"AMD Radeon Chill enabled but Radeon Anti-Lag is not supported or enabled.");
                }

                if (amdRadeonBoostSupported && amdRadeonBoostEnabled)
                {
                    Logger.Info($"AMD Radeon Chill enabled, Radeon Boost should be disabled too.");
                    amdRadeonBoostEnabled.SetValue(false);
                }
                else
                {
                    Logger.Info($"AMD Radeon Chill enabled but Radeon Boost is not supported or enabled.");
                }
            }
        }

        private void AmdRadeonBoostEnabled(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (amdRadeonBoostEnabled)
            {
                if (amdRadeonChillSupported && amdRadeonChillEnabled)
                {
                    Logger.Info($"Radeon Boost enabled, AMD Radeon Chill should be disabled too.");
                    amdRadeonChillEnabled.SetValue(false);
                }
                else
                {
                    Logger.Info($"Radeon Boost enabled but AMD Radeon Chill is not supported or enabled.");
                }
            }
        }

        private void AmdRadeonAntiLagEnabled(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (amdRadeonAntiLagEnabled)
            {
                if (amdRadeonChillSupported && amdRadeonChillEnabled)
                {
                    Logger.Info($"Radeon Anti-Lag enabled, AMD Radeon Chill should be disabled too.");
                    amdRadeonChillEnabled.SetValue(false);
                }
                else
                {
                    Logger.Info($"Radeon Anti-Lag enabled but AMD Radeon Chill is not supported or enabled.");
                }
            }
        }

        private void AmdFluidMotionFrameEnabled(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (amdFluidMotionFrameEnabled)
            {
                if (amdRadeonAntiLagSupported && !amdRadeonAntiLagEnabled)
                {
                    Logger.Info($"AMD Fluid Motion Frame enabled, Radeon Anti-Lag should be enabled too.");
                    amdRadeonAntiLagEnabled.SetValue(true);
                }
                else
                {
                    Logger.Info($"AMD Fluid Motion Frame enabled but Radeon Anti-Lag is not supported or already enabled.");
                }
            }
        }

        ~AMDManager()
        {
            adlxDisplayServices?.Dispose();
            adlxInternalGPU?.Dispose();
            adlxDedicatedGPU?.Dispose();
            adlxSecondDedicatedGPU?.Dispose();
            adlx3DSettingsServices?.Dispose();
            amdRadeonSuperResolutionSetting?.Dispose();
            amdFluidMotionFrameSetting?.Dispose();
            adlxHelper?.Dispose();
        }

        public override void Update()
        {
            base.Update();

            var (currentlyOn, currentLevel) = ReadCurrentMetricsProfile();
            if (onScreenDisplayLevel == 0)
            {
                if (currentlyOn == 1)
                {
                    if (!SettingsManager.GetInstance().IsForeground)
                    {
                        Logger.Info("Widget enters background, turn OFF On-Screen Display now.");
                        SendKeys.SendWait("^+o");
                    }
                    else
                    {
                        Logger.Info("On-Screen Display should be turned OFF but widget is still in foreground..");
                    }
                }
                else
                {
                    Logger.Info("On-Screen Display is already turned OFF.");
                }
            }
            else
            {
                if (currentlyOn == 0)
                {
                    if (!SettingsManager.GetInstance().IsForeground)
                    {
                        Logger.Info("Widget enters background, turn ON On-Screen Display now.");
                        SendKeys.SendWait("^+o");
                    }
                    else
                    {
                        Logger.Info("On-Screen Display should be turned ON but widget is still in foreground..");
                    }
                }


            }
        }

        public override void SetLevel(int level)
        {
            base.SetLevel(level);

            var current = ReadCurrentMetricsProfile();
            Logger.Info($"Set AMD Software: Adrenaline Edition performance overlay to {level}, current {current.Item1} - {current.Item2}");

            //User32.SendKeyCombo(0x11, 0x10, 0x4F);
            //SendKeys.SendWait("^+o");

            //var processes = Process.GetProcessesByName("RadeonSoftware");
            //if (processes.Length == 0)
            //{
            //    Logger.Warn("AMD Software: Adrenaline Edition is not running.");
            //    return;
            //}
            //var process = processes[0];

            //User32.PostMessage(process.MainWindowHandle, User32.WM_KEYDOWN, User32.VK_CONTROL, 0);
            //User32.PostMessage(process.MainWindowHandle, User32.WM_KEYDOWN, User32.VK_SHIFT, 0);
            //User32.PostMessage(process.MainWindowHandle, User32.WM_KEYDOWN, User32.VK_O, 0);
        }

        private static Tuple<int, int> ReadCurrentMetricsProfile()
        {
            try
            {
                using (RegistryKey subKey = AMD_PERFORMANCE_KEY_ROOT.OpenSubKey(AMD_PERFORMANCE_KEY_PATH))
                {
                    if (subKey != null)
                    {
                        object stateObject = subKey.GetValue(AMD_PERFORMANCE_STATE_KEY_NAME);

                        if (stateObject != null)
                        {
                            Logger.Info($"Value of '{AMD_PERFORMANCE_STATE_KEY_NAME}' at '{AMD_PERFORMANCE_KEY_PATH}': {stateObject} of type {stateObject.GetType().Name}");
                            var stateValue = (int)stateObject;
                            if (stateValue == 0)
                            {
                                return new Tuple<int, int>(0, 0);
                            }
                            else
                            {
                                var profileObject = subKey.GetValue(AMD_PERFORMANCE_PROFILE_KEY_NAME);
                                if (profileObject != null)
                                {
                                    Logger.Info($"Value of {AMD_PERFORMANCE_PROFILE_KEY_NAME} is {profileObject} of type {profileObject.GetType().Name}");
                                    var profileValue = (int)profileObject;
                                    return new Tuple<int, int>(stateValue, profileValue);
                                }
                                else
                                {
                                    return new Tuple<int, int>(stateValue, 0);
                                }
                            }
                        }
                        else
                        {
                            Logger.Info($"Value '{AMD_PERFORMANCE_STATE_KEY_NAME}' not found in '{AMD_PERFORMANCE_KEY_PATH}'.");
                            return new Tuple<int, int>(0, 0);
                        }
                    }
                    else
                    {
                        Logger.Info($"Registry key '{AMD_PERFORMANCE_KEY_PATH}' not found.");
                        return new Tuple<int, int>(0, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info($"An error occurred: {ex.Message}");
                return new Tuple<int, int>(0, 0);
            }
        }
    }
}
