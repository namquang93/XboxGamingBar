using NLog;
using Shared.Constants;
using Shared.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using XboxGamingBarHelper.AMD;
using XboxGamingBarHelper.Core;
using XboxGamingBarHelper.OnScreenDisplay;
using XboxGamingBarHelper.Hardware;
using XboxGamingBarHelper.Power;
using XboxGamingBarHelper.Profile;
using XboxGamingBarHelper.RTSS;
using XboxGamingBarHelper.Settings;
using XboxGamingBarHelper.Systems;

namespace XboxGamingBarHelper
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static AppServiceConnection connection = null;

        // Managers
        private static HardwareManager hardwareManager;
        private static RTSSManager rtssManager;
        private static ProfileManager profileManager;
        private static SystemManager systemManager;
        private static PowerManager powerManager;
        private static AMDManager amdManager;
        private static SettingsManager settingsManager;
        private static List<IManager> Managers;
        private static AppServiceConnectionStatus appServiceConnectionStatus;

        public static OnScreenDisplayProperty onScreenDisplay;
        public static List<OnScreenDisplayManager> onScreenDisplayProviders;

        // Properties
        private static HelperProperties properties;

        static async Task Main(string[] args)
        {
            await Initialize();
        }

        /// <summary>
        /// Open connection to UWP app service
        /// </summary>
        private static async Task Initialize()
        {
            // Initialize app service connection.
            InitializeConnection();

            //while (!System.Diagnostics.Debugger.IsAttached)
            //{
            //    await Task.Delay(500);
            //}

            // Initialize managers.
            Logger.Info("Initialize Settings Manager.");
            settingsManager = SettingsManager.CreateInstance(connection);

            Logger.Info("Initialize Hardware Manager.");
            hardwareManager = new HardwareManager(connection);
            Logger.Info("Initialize RTSS Manager.");
            rtssManager = new RTSSManager(hardwareManager, connection);
            Logger.Info("Initialize Profile Manager.");
            profileManager = new ProfileManager(connection);
            Logger.Info("Initialize System Manager.");
            systemManager = new SystemManager(connection, profileManager.GameProfiles);
            Logger.Info("Initialize Power Manager.");
            powerManager = new PowerManager(connection);
            Logger.Info("Initialize AMD Manager.");
            amdManager = new AMDManager(connection);
            
            Managers = new List<IManager>
            {
                hardwareManager,
                rtssManager,
                profileManager,
                systemManager,
                powerManager,
                amdManager,
                settingsManager
            };

            Logger.Info("Initialize properties.");
            onScreenDisplayProviders = new List<OnScreenDisplayManager>() { rtssManager, amdManager };
            onScreenDisplay = new OnScreenDisplayProperty(settingsManager.Setting.OnScreenDisplay, null, onScreenDisplayProviders[settingsManager.OnScreenDisplayProvider]);
            settingsManager.SyncOnScreenDisplaySettings(onScreenDisplay);
            //onScreenDisplay = new OnScreenDisplayProperty(0, null, amdManager);

            // Initialize properties.
            properties = new HelperProperties(
                systemManager.RunningGame,
                onScreenDisplay,
                hardwareManager.MinTDP,
                hardwareManager.MaxTDP,
                hardwareManager.TDPControlSupport,
                hardwareManager.TDP,
                profileManager.PerGameProfile,
                powerManager.CPUBoost,
                powerManager.CPUEPP,
                powerManager.LimitCPUClock,
                powerManager.CPUClockMax,
                systemManager.RefreshRates,
                systemManager.RefreshRate,
                systemManager.Resolutions,
                systemManager.Resolution,
                systemManager.TrackedGame,
                settingsManager.OnScreenDisplayProviderInstalled,
                settingsManager.IsForeground,
                amdManager.AMDSettingsSupported,
                amdManager.AMDRadeonSuperResolutionEnabled,
                amdManager.AMDRadeonSuperResolutionSupported,
                amdManager.AMDRadeonSuperResolutionSharpness,
                amdManager.AMDFluidMotionFrameEnabled,
                amdManager.AMDFluidMotionFrameSupported,
                amdManager.AMDRadeonAntiLagEnabled,
                amdManager.AMDRadeonAntiLagSupported,
                amdManager.AMDRadeonBoostEnabled,
                amdManager.AMDRadeonBoostSupported,
                amdManager.AMDRadeonBoostResolution,
                amdManager.AMDRadeonChillEnabled,
                amdManager.AMDRadeonChillSupported,
                amdManager.AMDRadeonChillMinFPS,
                amdManager.AMDRadeonChillMaxFPS,
                amdManager.FocusingOnOSDSlider,
                settingsManager.OnScreenDisplayProvider);

            Logger.Info("Initialize callbacks.");
            systemManager.RunningGame.PropertyChanged += RunningGame_PropertyChanged;
            systemManager.ResumeFromSleep += SystemManager_ResumeFromSleep;
            profileManager.PerGameProfile.PropertyChanged += PerGameProfile_PropertyChanged;
            hardwareManager.TDP.PropertyChanged += TDP_PropertyChanged;
            powerManager.CPUBoost.PropertyChanged += CPUBoost_PropertyChanged;
            powerManager.CPUEPP.PropertyChanged += CPUEPP_PropertyChanged;
            powerManager.LimitCPUClock.PropertyChanged += CPUClock_PropertyChanged;
            powerManager.CPUClockMax.PropertyChanged += CPUClock_PropertyChanged;
            profileManager.CurrentProfile.PropertyChanged += CurrentProfile_PropertyChanged;

            await ConnectToWidget(true);

            Logger.Info($"Widget connection status: {appServiceConnectionStatus}");
            while (true)
            {
                if (appServiceConnectionStatus != AppServiceConnectionStatus.Success)
                {
                    Logger.Info("Try to reconnect to the widget.");
                    await ConnectToWidget(false);
                }

                await Task.Delay(500);

                foreach (var manager in Managers)
                {
                    manager.Update();
                }
            }
        }

        private static void SystemManager_ResumeFromSleep(object sender)
        {
            Logger.Info("System resumed from sleep, re-apply current profile settings.");
            // Re-apply current profile settings.
            CurrentProfile_PropertyChanged(sender, null);
        }

        private static void InitializeConnection()
        {
            Logger.Info("Initialize connection...");
            connection = new AppServiceConnection();
            connection.AppServiceName = "XboxGamingBarService";
            connection.PackageFamilyName = Package.Current.Id.FamilyName;
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;
        }

        private static async Task ConnectToWidget(bool blocking)
        {
            if (blocking)
            {
                do
                {
                    Logger.Info("Start connecting to the widget.");
                    try
                    {
                        appServiceConnectionStatus = await connection.OpenAsync();
                    }
                    catch (Exception exception)
                    {
                        Logger.Error($"Exception occurred when connecting to the widget: {exception}");
                        appServiceConnectionStatus = AppServiceConnectionStatus.AppServiceUnavailable;
                    }

                    if (appServiceConnectionStatus != AppServiceConnectionStatus.Success)
                    {
                        Logger.Info("Can't conncect to the widget. Try again in 1 second...");
                        await Task.Delay(1000);
                    }
                } while (appServiceConnectionStatus != AppServiceConnectionStatus.Success);
                Logger.Info("Connected to the widget.");
            }
            else
            {
                Logger.Info("Start trying to connect to the widget.");
                try
                {
                    appServiceConnectionStatus = await connection.OpenAsync();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Exception occurred when trying to connect to the widget.");
                    appServiceConnectionStatus = AppServiceConnectionStatus.AppServiceUnavailable;
                }

                Logger.Info($"Try to conncect to the widget {appServiceConnectionStatus}.");
            }
        }

        private static void CPUClock_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var cpuClock = powerManager.LimitCPUClock ? powerManager.CPUClockMax : 0;
            Logger.Info($"Set current profile {profileManager.CurrentProfile.GameId.Name}'s CPU Clock from {profileManager.CurrentProfile.CPUClock} to {cpuClock}.");
            profileManager.CurrentProfile.CPUClock = cpuClock;
        }

        private static void CPUBoost_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Logger.Info($"Set current profile {profileManager.CurrentProfile.GameId.Name}'s CPU Boost from {profileManager.CurrentProfile.CPUBoost} to {powerManager.CPUBoost}.");
            profileManager.CurrentProfile.CPUBoost = powerManager.CPUBoost;
        }

        private static void CPUEPP_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Logger.Info($"Set current profile {profileManager.CurrentProfile.GameId.Name}'s CPU EPP from {profileManager.CurrentProfile.CPUEPP} to {powerManager.CPUEPP}.");
            profileManager.CurrentProfile.CPUEPP = powerManager.CPUEPP;
        }

        private static void CurrentProfile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (profileManager.CurrentProfile.Use || profileManager.CurrentProfile.IsGlobalProfile)
            {
                Logger.Info($"Profile changed to {profileManager.CurrentProfile.GameId.Name}, apply it.");
                hardwareManager.TDP.SetValue(profileManager.CurrentProfile.TDP);
                powerManager.CPUBoost.SetValue(profileManager.CurrentProfile.CPUBoost);
                powerManager.CPUEPP.SetValue(profileManager.CurrentProfile.CPUEPP);
                powerManager.LimitCPUClock.SetValue(profileManager.CurrentProfile.CPUClock > 0);
                powerManager.CPUClockMax.SetValue(profileManager.CurrentProfile.CPUClock > 0 ? profileManager.CurrentProfile.CPUClock : CPUConstants.DEFAULT_CPU_CLOCK);
                profileManager.PerGameProfile.SetValue(profileManager.CurrentProfile.Use);
            }
            else
            {
                Logger.Info($"Profile changed to {profileManager.CurrentProfile.GameId.Name} is not used.");
            }
        }

        private static void PerGameProfile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            GameProfile gameProfile;
            if (profileManager.PerGameProfile)
            {
                if (!profileManager.TryGetProfile(systemManager.RunningGame.Value.GameId, out gameProfile))
                {
                    gameProfile = profileManager.AddNewProfile(systemManager.RunningGame.Value.GameId);
                }
                Logger.Info($"Enable per-game profile for {systemManager.RunningGame.Value.GameId}");
                gameProfile.Use = true;
            }
            else
            {
                if (profileManager.TryGetProfile(systemManager.RunningGame.Value.GameId, out gameProfile))
                {
                    gameProfile.Use = false;
                }
                gameProfile = profileManager.GlobalProfile;
            }
            profileManager.CurrentProfile.SetValue(gameProfile);
        }

        private static void TDP_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Logger.Info($"Set current profile {profileManager.CurrentProfile.GameId.Name}'s TDP from {profileManager.CurrentProfile.TDP} to {hardwareManager.TDP}.");
            profileManager.CurrentProfile.TDP = hardwareManager.TDP;
        }

        private static void RunningGame_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (systemManager.RunningGame.Value.IsValid())
            {
                if (profileManager.TryGetProfile(systemManager.RunningGame.Value.GameId, out var runningGameProfile))
                {
                    if (runningGameProfile.Use)
                    {
                        Logger.Info($"Game {systemManager.RunningGame.GameId} has per-game profile in use.");
                        profileManager.CurrentProfile.SetValue(runningGameProfile);
                    }
                    else
                    {
                        Logger.Info($"Game {systemManager.RunningGame.GameId} has per-game profile but not in use.");
                    }
                }
                else
                {
                    Logger.Info($"Game {systemManager.RunningGame.GameId} doesn't have per-game profile.");
                }
            }
            else
            {
                Logger.Info($"Stopped playing game, use global profile instead.");
                profileManager.CurrentProfile.SetValue(profileManager.GlobalProfile);
            }
        }

        /// <summary>
        /// Handles the event when the desktop process receives a request from the UWP app
        /// </summary>
        private static async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            Logger.Info($"Helper received message {args.Request.Message.ToDebugString()} from widget.");
            await properties.OnRequestReceived(args.Request);
        }

        /// <summary>
        /// Handles the event when the app service connection is closed
        /// </summary>
        private static void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            Logger.Info("Lost connection to the widget.");
            appServiceConnectionStatus = AppServiceConnectionStatus.AppServiceUnavailable;

            Logger.Info("Prepare to re-connect to the widget.");
            try
            {
                connection?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error($"Exception occurred when disposing the connection: {ex}");
            }
            InitializeConnection();
            foreach (var manager in Managers)
            {
                manager.Connection = connection;
            }
        }
    }
}
