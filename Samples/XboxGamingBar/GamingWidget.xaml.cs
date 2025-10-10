using NLog;
using Shared.Data;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using XboxGamingBar.Data;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace XboxGamingBar
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamingWidget : Page
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly OSDProperty osdProperty;
        private readonly TDPProperty tdpProperty;
        private readonly List<FunctionalProperty> properties;

        public GamingWidget()
        {
            InitializeComponent();
            tdpProperty = new TDPProperty(4, TDPSlider, this);
            osdProperty = new OSDProperty(0, PerformanceOverlaySlider, this);
            properties = new List<FunctionalProperty>()
            {
                tdpProperty,
                osdProperty,
            };
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //while (!System.Diagnostics.Debugger.IsAttached)
            //{
            //    await Task.Delay(500);
            //}

            if (App.Connection == null && ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                Logger.Info("Launching a new full trust process.");
                App.AppServiceConnected += GamingWidget_AppServiceConnected;
                App.AppServiceDisconnected += GamingWidget_AppServiceDisconnected;
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            }
        }

        public async Task GamingWidget_LeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            if (App.Connection != null)
            {
                Logger.Info("GamingWidget LeavingBackground, sync UI now.");
                await SyncProperties();
            }
            else
            {
                Logger.Info("GamingWidget LeavingBackground but not connected to the full trust process");
            }
        }

        public void GamingWidget_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            Logger.Info("GamingWidget EnterBackground");
        }

        /// <summary>
        /// When the desktop process is connected, get ready to send/receive requests
        /// </summary>
        private async void GamingWidget_AppServiceConnected(object sender, AppServiceTriggerDetails e)
        {
            App.Connection.RequestReceived += AppServiceConnection_RequestReceived;
            await SyncProperties();

            //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //{
            //    Logger.Info("AppService Connected");
            //    // enable UI to access  the connection
            //    // btnRegKey.IsEnabled = true;
            //});
        }

        private async Task SyncProperties()
        {
            foreach (var property in properties)
            {
                await property.SyncProperty();
            }
        }

        private IAsyncAction SyncRunningGame(RunningGame runningGame)
        {
            //RunningGame = runningGame;
            return Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (runningGame.IsValid())
                {
                    CurrentGameText.Text = $"{runningGame.GameId.Name}{(runningGame.IsForeground ? string.Empty : "*")}";
                    GameProfileToggle.Visibility = Visibility.Visible;
                }
                else
                {
                    CurrentGameText.Text = $"No game detected";
                    GameProfileToggle.Visibility = Visibility.Collapsed;
                }
            });
        }

        /// <summary>
        /// When the desktop process is disconnected, reconnect if needed
        /// </summary>
        private async void GamingWidget_AppServiceDisconnected(object sender, EventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Logger.Info("AppService Disconnected");
                // disable UI to access the connection
                // btnRegKey.IsEnabled = false;

                // ask user if they want to reconnect
                // Reconnect();
            });
        }

        /// <summary>
        /// Handle calculation request from desktop process
        /// (dummy scenario to show that connection is bi-directional)
        /// </summary>
        private async void AppServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            Logger.Info($"Receive message {args.Request.Message.ToString()} from helper.");

            Command command;
            if (!args.Request.Message.TryGetValue(nameof(Command), out var commandObject))
            {
                Logger.Error("Invalid message command.");
                command = Command.Get;
            }
            else
            {
                command = (Command)commandObject;
            }

            Function function;
            if (!args.Request.Message.TryGetValue(nameof(Function), out var functionObject))
            {
                Logger.Error("Invalid message function.");
                function = Function.TDP;
            }
            else
            {
                function = (Function)functionObject;
            }

            if (!args.Request.Message.TryGetValue(nameof(Content), out var valueObject))
            {
                Logger.Error("Invalid message value.");
            }
            

            var result = "fail";
            switch (command)
            {
                case Command.Get:
                    Logger.Error("Received invalid get command");
                    break;
                case Command.Set:
                    switch (function)
                    {
                        case Function.TDP:
                            result = "success";
                            //TDP = (int)valueObject;
                            break;
                        case Function.CurrentGame:
                            result = "success";
                            var stringValue = (string)valueObject;
                            await SyncRunningGame(RunningGame.FromString(stringValue));
                            break;
                    }
                    break;
            }
            ValueSet response = new ValueSet
            {
                { nameof(Content), result }
            };
            await args.Request.SendResponseAsync(response);
        }

        private async void PerformanceOverlaySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var level = (int)e.NewValue;

            if (App.Connection != null)
            {
                ValueSet request = new ValueSet
                {
                    { nameof(Command), (int)Command.Set },
                    { nameof(Function), (int)Function.OSD },
                    { nameof(Content), level }
                };
                AppServiceResponse response = await App.Connection.SendMessageAsync(request);
                if (response != null)
                {
                    Logger.Info($"Set OSD level {level} to desktop process");
                }
                else
                {
                    Logger.Info($"No response from desktop process when trying to change OSD level {level}.");
                }
            }
            else
            {
                Logger.Info("No connection for performance overlay!");
            }
        }

        private async void GameProfileToggle_Toggled(object sender, RoutedEventArgs e)
        {
            //if (!RunningGame.IsValid())
            //{
            //    Logger.Warn("Running game is invaid, can't change profile.");
            //    return;
            //}

            if (App.Connection == null)
            {
                Logger.Warn("No connection for game profile!");
                return;
            }

            ValueSet request = new ValueSet
            {
                { nameof(Command), (int)Command.Set },
                { nameof(Function), (int)Function.GameProfile },
                { nameof(Content), GameProfileToggle.IsOn }
            };
            AppServiceResponse response = await App.Connection.SendMessageAsync(request);
            if (response != null)
            {
                Logger.Info($"Set game profile  to desktop process");
            }
            else
            {
                Logger.Info($"Can't save per-game profile.");
            }
        }
    }
}
