using Microsoft.Gaming.XboxGameBar;
using NLog;
using Shared.Data;
using Shared.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Input;
using Windows.System;
using XboxGamingBar.Data;
using XboxGamingBar.Event;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace XboxGamingBar
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamingWidget : Page
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly List<string> BlackListAppTrackerNames = new List<string>()
        {
            "App Installer", //Somehow App Installer shows up as a game sometimes
        };

        // Xbox Game Bar logic
        private XboxGameBarWidget widget = null;
        private XboxGameBarWidgetActivity widgetActivity = null;
        public XboxGameBarWidgetActivity WidgetActivity { get { return widgetActivity; } }
        private XboxGameBarAppTargetTracker appTargetTracker = null;

        private SolidColorBrush widgetDarkThemeBrush = null;
        private SolidColorBrush widgetLightThemeBrush = null;

        // Properties
        private readonly OSDProperty osd;
        private readonly MinTDPProperty minTDP;
        private readonly MaxTDPProperty maxTDP;
        private readonly TDPControlSupportProperty tdpControlSupport;
        private readonly TDPProperty tdp;
        private readonly RunningGameProperty runningGame;
        private readonly PerGameProfileProperty perGameProfile;
        private readonly CPUBoostProperty cpuBoost;
        private readonly CPUEPPProperty cpuEPP;
        private readonly LimitCPUClockProperty limitCPUClock;
        private readonly CPUClockMaxProperty cpuClockMax;
        private readonly RefreshRatesProperty refreshRates;
        private readonly RefreshRateProperty refreshRate;
        private readonly ResolutionProperty resolution;
        private readonly ResolutionsProperty resolutions;
        private readonly TrackedGameProperty trackedGame;
        private readonly OnScreenDisplayProviderInstalledProperty onScreenDisplayProviderInstalled;
        private readonly IsForegroundProperty isForeground;
        private readonly FocusingOnOSDSliderProperty focusingOnOSDSlider;
        private readonly LosslessScalingShortcutProperty losslessScalingShortcut;

        // AMD properties
        private readonly AMDSettingsSupportedProperty amdSettingsSupported;
        private readonly AMDRadeonSuperResolutionEnabledProperty amdRadeonSuperResolutionEnabled;
        private readonly AMDRadeonSuperResolutionSupportedProperty amdRadeonSuperResolutionSupported;
        private readonly AMDRadeonSuperResolutionSharpnessProperty amdRadeonSuperResolutionSharpness;
        private readonly AMDFluidMotionFrameEnabledProperty amdFluidMotionFrameEnabled;
        private readonly AMDFluidMotionFrameSupportedProperty amdFluidMotionFrameSupported;
        private readonly AMDRadeonAntiLagEnabledProperty amdRadeonAntiLagEnabled;
        private readonly AMDRadeonAntiLagSupportedProperty amdRadeonAntiLagSupported;
        private readonly AMDRadeonBoostEnabledProperty amdRadeonBoostEnabled;
        private readonly AMDRadeonBoostSupportedProperty amdRadeonBoostSupported;
        private readonly AMDRadeonBoostResolutionProperty amdRadeonBoostResolution;
        private readonly AMDRadeonChillEnabledProperty amdRadeonChillEnabled;
        private readonly AMDRadeonChillSupportedProperty amdRadeonChillSupported;
        private readonly AMDRadeonChillMinFPSProperty amdRadeonChillMinFPSProperty;
        private readonly AMDRadeonChillMaxFPSProperty amdRadeonChillMaxFPSProperty;

        private readonly IsListeningForKeyBindingProperty isListeningForKeyBinding;

        private readonly WidgetProperties properties;

        //private bool isListeningForKeyBinding = false;
        private bool isFirstKeyCaptured = false;

        public GamingWidget()
        {
            InitializeComponent();
            minTDP = new MinTDPProperty(TDPSlider, this);
            maxTDP = new MaxTDPProperty(TDPSlider, this);
            tdpControlSupport = new TDPControlSupportProperty(TDPSlider, this, TDPHeaderGrid);
            tdp = new TDPProperty(4, TDPSlider, this);
            osd = new OSDProperty(0, PerformanceOverlaySlider, this);
            runningGame = new RunningGameProperty(RunningGameText, PerGameProfileToggle, this);
            perGameProfile = new PerGameProfileProperty(PerGameProfileToggle, this);
            cpuBoost = new CPUBoostProperty(CPUBoostToggle, this);
            cpuEPP = new CPUEPPProperty(80, CPUEPPSlider, this);
            limitCPUClock = new LimitCPUClockProperty(LimitCPUClockToggle, this);
            cpuClockMax = new CPUClockMaxProperty(CPUClockMaxSlider, this);
            refreshRates = new RefreshRatesProperty(RefreshRatesComboBox, this);
            refreshRate = new RefreshRateProperty(RefreshRatesComboBox, this);
            resolutions = new ResolutionsProperty(ResolutionsComboBox, this);
            resolution = new ResolutionProperty(ResolutionsComboBox, this);
            trackedGame = new TrackedGameProperty(new TrackedGame());
            onScreenDisplayProviderInstalled = new OnScreenDisplayProviderInstalledProperty(PerformanceOverlaySlider, this);
            isForeground = new IsForegroundProperty();
            amdSettingsSupported = new AMDSettingsSupportedProperty(AMDPivotItem, this, AMDPivotItemStackPanel, AMDRadeonSuperResolutionToggle,
                AMDRadeonSuperResolutionText, AMDFluidMotionFrameToggle, AMDFluidMotionFrameText, AMDRadeonAntiLagToggle, AMDRadeonAntiLagText,
                AMDRadeonBoostToggle, AMDRadeonBoostText, AMDRadeonChillToggle, AMDRadeonChillText);
            amdRadeonSuperResolutionEnabled = new AMDRadeonSuperResolutionEnabledProperty(AMDRadeonSuperResolutionToggle, this);
            amdRadeonSuperResolutionSupported = new AMDRadeonSuperResolutionSupportedProperty(AMDRadeonSuperResolutionToggle, this);
            amdRadeonSuperResolutionSharpness = new AMDRadeonSuperResolutionSharpnessProperty(AMDRadeonSuperResolutionSharpnessSlider, this);
            amdFluidMotionFrameEnabled = new AMDFluidMotionFrameEnabledProperty(AMDFluidMotionFrameToggle, this);
            amdFluidMotionFrameSupported = new AMDFluidMotionFrameSupportedProperty(AMDFluidMotionFrameToggle, this);
            amdRadeonAntiLagEnabled = new AMDRadeonAntiLagEnabledProperty(AMDRadeonAntiLagToggle, this);
            amdRadeonAntiLagSupported = new AMDRadeonAntiLagSupportedProperty(AMDRadeonAntiLagToggle, this);
            amdRadeonBoostEnabled = new AMDRadeonBoostEnabledProperty(AMDRadeonBoostToggle, this);
            amdRadeonBoostSupported = new AMDRadeonBoostSupportedProperty(AMDRadeonBoostToggle, this);
            amdRadeonBoostResolution = new AMDRadeonBoostResolutionProperty(AMDRadeonBoostResolutionSlider, this);
            amdRadeonChillEnabled = new AMDRadeonChillEnabledProperty(AMDRadeonChillToggle, this);
            amdRadeonChillSupported = new AMDRadeonChillSupportedProperty(AMDRadeonChillToggle, this);
            amdRadeonChillMinFPSProperty = new AMDRadeonChillMinFPSProperty(AMDRadeonChillMinFPSSlider, this);
            amdRadeonChillMaxFPSProperty = new AMDRadeonChillMaxFPSProperty(AMDRadeonChillMaxFPSSlider, this);
            focusingOnOSDSlider = new FocusingOnOSDSliderProperty(PerformanceOverlaySlider, this);
            isListeningForKeyBinding = new IsListeningForKeyBindingProperty();
            losslessScalingShortcut = new LosslessScalingShortcutProperty(LosslessScalingBindingButton, new List<int>());

            properties = new WidgetProperties(
                osd,
                minTDP,
                maxTDP,
                tdpControlSupport,
                tdp,
                runningGame,
                perGameProfile,
                cpuBoost,
                cpuEPP,
                limitCPUClock,
                cpuClockMax,
                refreshRates,
                refreshRate,
                resolutions,
                resolution,
                trackedGame,
                onScreenDisplayProviderInstalled,
                isForeground,
                amdSettingsSupported,
                amdRadeonSuperResolutionEnabled,
                amdRadeonSuperResolutionSupported,
                amdRadeonSuperResolutionSharpness,
                amdFluidMotionFrameEnabled,
                amdFluidMotionFrameSupported,
                amdRadeonAntiLagEnabled,
                amdRadeonAntiLagSupported,
                amdRadeonBoostEnabled,
                amdRadeonBoostSupported,
                amdRadeonBoostResolution,
                amdRadeonChillEnabled,
                amdRadeonChillSupported,
                amdRadeonChillMinFPSProperty,
                amdRadeonChillMaxFPSProperty,
                focusingOnOSDSlider,
                isListeningForKeyBinding,
                losslessScalingShortcut
            );

            this.KeyDown += GamingWidget_KeyDown;
            this.MainPivot.SelectionChanged += MainPivot_SelectionChanged;
            this.LosslessScalingBindingButton.LostFocus += LosslessScalingBindingButton_LostFocus;
        }

        private void LosslessScalingBindingButton_LostFocus(object sender, RoutedEventArgs e)
        {
            CancelListening();
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CancelListening();
        }

        private void CancelListening()
        {
            if (isListeningForKeyBinding)
            {
                isListeningForKeyBinding.SetValue(false);
                if (!isFirstKeyCaptured)
                {
                    losslessScalingShortcut.RefreshUI();
                }
                LosslessScalingBindingButton.IsEnabled = true;
            }
        }

        private void GamingWidget_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (isListeningForKeyBinding)
            {
                // Capture the gamepad key
                if (e.Key.ToString().Contains("Gamepad"))
                {
                    // Allow navigation if no key has been captured yet? 
                    // Or prioritize capturing. Let's allow DPad/Stick navigation to cancel listening.
                    if (e.Key == VirtualKey.GamepadDPadUp || e.Key == VirtualKey.GamepadDPadDown || 
                        e.Key == VirtualKey.GamepadDPadLeft || e.Key == VirtualKey.GamepadDPadRight ||
                        e.Key == VirtualKey.GamepadLeftThumbstickUp || e.Key == VirtualKey.GamepadLeftThumbstickDown ||
                        e.Key == VirtualKey.GamepadLeftThumbstickLeft || e.Key == VirtualKey.GamepadLeftThumbstickRight)
                    {
                        CancelListening();
                        return; // Let the event bubble up for navigation
                    }

                    List<int> keys;
                    if (!isFirstKeyCaptured)
                    {
                        keys = new List<int>();
                        isFirstKeyCaptured = true;
                    }
                    else
                    {
                        keys = new List<int>(losslessScalingShortcut.Value);
                    }

                    if (!keys.Contains((int)e.Key))
                    {
                        keys.Add((int)e.Key);
                        losslessScalingShortcut.SetValue(keys);
                    }
                    e.Handled = true;
                    // We stop listening after a short delay
                    StopListeningWithDelay();
                }
                return;
            }

            if (e.Key == VirtualKey.GamepadLeftTrigger)
            {
                NavigatePivot(-1);
                e.Handled = true;
            }
            else if (e.Key == VirtualKey.GamepadRightTrigger)
            {
                NavigatePivot(1);
                e.Handled = true;
            }
        }

        private async void StopListeningWithDelay()
        {
            int currentKeyCount = losslessScalingShortcut.Value.Count;
            await Task.Delay(1000);
            if (losslessScalingShortcut.Value.Count == currentKeyCount && isListeningForKeyBinding)
            {
                isListeningForKeyBinding.SetValue(false);
                LosslessScalingBindingButton.IsEnabled = true;
            }
        }

        private void LosslessScalingBindingButton_Click(object sender, RoutedEventArgs e)
        {
            isListeningForKeyBinding.SetValue(true);
            isFirstKeyCaptured = false;
            
            StartListeningTimeout();
        }

        private async void StartListeningTimeout()
        {
            // Give the user 5 seconds to press any gamepad key
            for (int i = 5; i > 0; i--)
            {
                if (!isListeningForKeyBinding || isFirstKeyCaptured)
                {
                    return;
                }
                LosslessScalingBindingButton.Content = i.ToString();
                await Task.Delay(1000);
            }

            if (isListeningForKeyBinding && !isFirstKeyCaptured)
            {
                isListeningForKeyBinding.SetValue(false);
                losslessScalingShortcut.RefreshUI();
            }
        }

        private void NavigatePivot(int direction)
        {
            int count = MainPivot.Items.Count;
            if (count <= 1) return;

            int currentIndex = MainPivot.SelectedIndex;
            int nextIndex = currentIndex;

            // Try to find the next visible PivotItem
            for (int i = 0; i < count; i++)
            {
                nextIndex = (nextIndex + direction + count) % count;
                if (MainPivot.Items[nextIndex] is PivotItem item && item.Visibility == Visibility.Visible)
                {
                    MainPivot.SelectedIndex = nextIndex;
                    break;
                }

                // If we've circled back to current, stop
                if (nextIndex == currentIndex) break;
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //while (!System.Diagnostics.Debugger.IsAttached)
            //{
            //    await Task.Delay(500);
            //}

            widgetDarkThemeBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 37, 40, 44));
            widgetLightThemeBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));

            widget = e.Parameter as XboxGameBarWidget;
            if (widget != null)
            {
                Logger.Info("Running as a Xbox Game Bar widget.");
                await widget.CenterWindowAsync();
                widget.RequestedThemeChanged += GamingWidget_RequestedThemeChanged;
                widget.SettingsClicked += GamingWidget_SettingsClicked;
            }
            else
            {
                Logger.Info("XboxGameBarWidget not available, probably running as an app instead of widget.");
            }

            Logger.Info($"App.Connection:{(App.Connection == null ? "NULL" : "NOT_NULL")} FullTrustAppContract:{(ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0) ? "PRESENT" : "NOT_PRESENT")}");
            if (App.Connection != null)
            {
                ReconnectAppService();
            }
            else
            {
                Logger.Info("Wait 1 second for the helper to reconnect...");
                await Task.Delay(1000);
                Logger.Info($"After 1 second: App.Connection:{(App.Connection == null ? "NULL" : "NOT_NULL")} FullTrustAppContract:{(ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0) ? "PRESENT" : "NOT_PRESENT")}");

                if (App.Connection == null && ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
                {
                    Logger.Info("Launching a new full trust process.");
                    App.AppServiceConnected += GamingWidget_AppServiceConnected;
                    App.AppServiceDisconnected += GamingWidget_AppServiceDisconnected;
                    await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
                }
                else
                {
                    ReconnectAppService();
                }
            }
        }

        private void ReconnectAppService()
        {
            Logger.Info("Reconnect to existing AppServiceConnection.");
            App.AppServiceConnected -= GamingWidget_AppServiceConnected;
            App.AppServiceConnected += GamingWidget_AppServiceConnected;
            App.AppServiceDisconnected -= GamingWidget_AppServiceDisconnected;
            App.AppServiceDisconnected += GamingWidget_AppServiceDisconnected;
            GamingWidget_AppServiceConnected(null, null);
        }

        public async Task GamingWidget_LeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            if (widget != null)
            {
                await widget.CenterWindowAsync();
            }

            if (App.Connection != null)
            {
                Logger.Info("GamingWidget LeavingBackground, sync UI now.");
                await properties.Sync();
            }
            else
            {
                Logger.Info("GamingWidget LeavingBackground but not connected to the full trust process.");
            }

            isForeground.SetValue(true);
        }

        public void GamingWidget_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            Logger.Info("GamingWidget EnterBackground.");
            isForeground.SetValue(false);
        }

        private void AppTargetTracker_TargetChanged(XboxGameBarAppTargetTracker sender, object args)
        {
            var settingEnabled = appTargetTracker.Setting == XboxGameBarAppTargetSetting.Enabled;

            XboxGameBarAppTarget target = null;
            if (settingEnabled)
            {
                target = appTargetTracker.GetTarget();
            }

            if (target == null)
            {
                Logger.Debug("Found no target.");
                trackedGame.SetValue(new TrackedGame());
            }
            else
            {
                if (target.IsGame && !BlackListAppTrackerNames.Contains(target.DisplayName))
                {
                    Logger.Debug($"Tracked game DisplayName={target.DisplayName} AumId={target.AumId} TitleId={target.TitleId} IsFullscreen={target.IsFullscreen}");
                    trackedGame.SetValue(new TrackedGame(target.AumId, target.DisplayName, StringHelper.CleanStringForSerialization(target.TitleId), target.IsFullscreen));
                }
                else
                {
                    Logger.Debug($"Tracked non-game DisplayName={target.DisplayName} AumId={target.AumId} TitleId={target.TitleId} IsFullscreen={target.IsFullscreen}");
                    trackedGame.SetValue(new TrackedGame());
                }
            }
        }

        /// <summary>
        /// When the desktop process is connected, get ready to send/receive requests
        /// </summary>
        private async void GamingWidget_AppServiceConnected(object sender, AppServiceTriggerDetails _)
        {
            Logger.Info("GamingWidget AppService connected.");
            if (widget != null)
            {
                if (widgetActivity == null)
                {
                    try
                    {
                        widgetActivity = new XboxGameBarWidgetActivity(widget, "XboxGamingBarActivity");
                        Logger.Info("Create new activity to keep the widget runs in the background.");
                    }
                    catch (ArgumentException argumentException)
                    {
                        Logger.Warn($"Can't create widget acitvity: {argumentException}.");
                    }
                }
                else
                {
                    Logger.Info("Widget activity already created.");
                }

                if (appTargetTracker == null)
                {
                    appTargetTracker = new XboxGameBarAppTargetTracker(widget);
                    appTargetTracker.SettingChanged += AppTargetTracker_TargetChanged;

                    if (appTargetTracker.Setting == XboxGameBarAppTargetSetting.Enabled)
                    {
                        Logger.Info("Created new app target tracker to track current game.");
                        var initialTarget = appTargetTracker.GetTarget();
                        if (initialTarget.IsGame)
                        {
                            Logger.Info($"Initial tracked game DisplayName={initialTarget.DisplayName} AumId={initialTarget.AumId} TitleId={initialTarget.TitleId} IsFullscreen={initialTarget.IsFullscreen}");
                            trackedGame.SetValue(new TrackedGame(initialTarget.AumId, initialTarget.DisplayName, StringHelper.CleanStringForSerialization(initialTarget.TitleId), initialTarget.IsFullscreen));
                        }
                        else
                        {
                            trackedGame.SetValue(new TrackedGame());
                            Logger.Info("No initial game target found.");
                        }
                        appTargetTracker.TargetChanged += AppTargetTracker_TargetChanged;
                    }
                    else
                    {
                        Logger.Info("Created new app target tracker but not enabled.");
                    }
                }
                else
                {
                    Logger.Info("App target tracker already created.");
                }
            }
            else
            {
                Logger.Info("No widget found, probably running as an app instead of Xbox Game Bar widget.");
            }

            await properties.Sync();
        }

        /// <summary>
        /// When the desktop process is disconnected, reconnect if needed
        /// </summary>
        private async void GamingWidget_AppServiceDisconnected(object sender, EventArgs e)
        {
            if (widgetActivity != null)
            {
                widgetActivity.Complete();
                widgetActivity = null;
                Logger.Info("Stopped widget activity.");
            }

            //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //{
            //    Logger.Info("AppService disconnected, disable UI elements");
            //    PerformanceOverlaySlider.IsEnabled = false;
            //    TDPSlider.IsEnabled = false;
            //    PerGameProfileToggle.IsEnabled = false;
            //    CPUBoostToggle.IsEnabled = false;
            //    CPUEPPSlider.IsEnabled = false;
            //    LimitCPUClockToggle.IsEnabled = false;
            //    CPUClockMaxSlider.IsEnabled = false;
            //    RefreshRatesComboBox.IsEnabled = false;
            //    AMDRadeonSuperResolutionToggle.IsEnabled = false;
            //    AMDRadeonSuperResolutionSharpnessSlider.IsEnabled = false;
            //    AMDFluidMotionFrameToggle.IsEnabled = false;
            //    AMDRadeonAntiLagToggle.IsEnabled = false;
            //    AMDRadeonBoostToggle.IsEnabled = false;
            //    AMDRadeonBoostResolutionSlider.IsEnabled = false;
            //    AMDRadeonChillToggle.IsEnabled = false;
            //    AMDRadeonChillMinFPSSlider.IsEnabled = false;
            //    AMDRadeonChillMaxFPSSlider.IsEnabled = false;
            //});

            var eventArgs = e as BackgroundTaskCancellationEventArgs;
            if (eventArgs != null && eventArgs.Reason != BackgroundTaskCancellationReason.Terminating)
            {
                Logger.Info($"AppService disconnected due to {eventArgs.Reason}, trying to relaunch.");
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            }
            else
            {
                Logger.Info($"AppService disconnected due to {eventArgs.Reason}, not relaunching.");
            }
        }

        private async void GamingWidget_RequestedThemeChanged(XboxGameBarWidget sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetBackgroundColor();
            });
        }

        private async void GamingWidget_SettingsClicked(XboxGameBarWidget sender, object args)
        {
            await widget.ActivateSettingsAsync();
        }

        private void SetBackgroundColor()
        {
            this.RequestedTheme = widget.RequestedTheme;
            RootGrid.Background = (widget.RequestedTheme == ElementTheme.Dark) ? widgetDarkThemeBrush : widgetLightThemeBrush;
        }

        /// <summary>
        /// Handle calculation request from desktop process
        /// (dummy scenario to show that connection is bi-directional)
        /// </summary>
        public async Task RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            Logger.Info($"GamingWidget received message {args.Request.Message.ToDebugString()} from helper.");
            await properties.OnRequestReceived(args.Request);
        }
    }
}
