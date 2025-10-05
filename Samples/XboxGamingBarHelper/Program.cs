using NLog;
using Shared.Data;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using XboxGamingBarHelper.Core;
using XboxGamingBarHelper.Performance;
using XboxGamingBarHelper.Profile;
using XboxGamingBarHelper.RTSS;
using XboxGamingBarHelper.System;

namespace XboxGamingBarHelper
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static AppServiceConnection connection = null;

        private static PerformanceManager performanceManager;
        private static RTSSManager rtssManager;
        private static ProfileManager profileManager;
        private static SystemManager systemManager;
        private static List<IManager> Managers = new List<IManager>();

        static async Task Main(string[] args)
        {
            // Console.Title = "Xbox Gaming Bar Helper";
            // Console.WriteLine($"OSD {OSD.GetOSDEntries().Length} APP {OSD.GetAppEntries().Length}");
            // Console.WriteLine("\r\nPress any key to exit ...");
            // Console.ReadLine();

            // await InitializeAppServiceConnection();


            Initialize();
            await InitializeAppServiceConnection();
        }

        private static void Initialize()
        {
            performanceManager = new PerformanceManager();
            Managers.Add(performanceManager);

            rtssManager = new RTSSManager(performanceManager);
            Managers.Add(rtssManager);

            profileManager = new ProfileManager();
            Managers.Add(profileManager);

            systemManager = new SystemManager();
            Managers.Add(systemManager);
            systemManager.RunningGameChanged += OnRunningGameChanged;
        }

        private static async void OnRunningGameChanged(object sender, RunningGameChangedEventArgs e)
        {
            if (connection == null)
            {
                Logger.Info("No connection yet, can't broadcast running game changed.");
                return;
            }

            ValueSet request = new ValueSet
            {
                { nameof(Command), (int)Command.Update },
                { nameof(Function),(int)Function.CurrentGame },
                { nameof(Value), e.NewRunningGame.ToString() }
            };
            AppServiceResponse response = await connection.SendMessageAsync(request);
            if (response.Message.TryGetValue(nameof(Value), out var result))
            {
                Logger.Info($"Update running game {(string)result}.");
            }
            else
            {
                Logger.Info("Can't update current game.");
            }

            if (!ProfileManager.TryLoadGameProfile(new GameProfileKey(e.NewRunningGame.Name, e.NewRunningGame.Path), out GameProfile gameProfile))
            {
                gameProfile = profileManager.GlobalProfile;
                Logger.Info($"New running game {e.NewRunningGame.Name} doesn't have profile, use global profile.");
            }

            profileManager.CurrentProfile = gameProfile;
            if (gameProfile.Use)
            {
                Logger.Info($"New running game {e.NewRunningGame.Name} have profile in used, apply it.");
                request = new ValueSet
                    {
                        { nameof(Command), (int)Command.Update },
                        { nameof(Function),(int)Function.TDP },
                        { nameof(Value), gameProfile.TDP }
                    };
                response = await connection.SendMessageAsync(request);
                if (response.Message.TryGetValue(nameof(Value), out result))
                {
                    Logger.Info($"Update game profile TDP {(string)result}.");
                }
                else
                {
                    Logger.Info("Can't update game profile TDP.");
                }
            }
            else
            {
                Logger.Info($"New running game {e.NewRunningGame.Name} have profile in used, do not apply it.");
            }
        }

        /// <summary>
        /// Open connection to UWP app service
        /// </summary>
        private static async Task InitializeAppServiceConnection()
        {
            connection = new AppServiceConnection();
            connection.AppServiceName = "XboxGamingBarService";
            connection.PackageFamilyName = Package.Current.Id.FamilyName;
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;

            AppServiceConnectionStatus status = await connection.OpenAsync();
            if (status != AppServiceConnectionStatus.Success)
            {
                return;
            }

            while (true)
            {
                await Task.Delay(500);

                foreach (var manager in Managers)
                {
                    manager.Update();
                }
            }
        }

        /// <summary>
        /// Handles the event when the desktop process receives a request from the UWP app
        /// </summary>
        private static async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var action = (Command)args.Request.Message[nameof(Command)];
            var function = (Function)args.Request.Message[nameof(Function)];
            switch (action)
            {
                case Command.Get:
                    ValueSet response = new ValueSet();
                    switch (function)
                    {
                        case Function.OSD:
                            response.Add(nameof(Value), rtssManager.OSDLevel);
                            break;
                        case Function.TDP:
                            response.Add(nameof(Value), performanceManager.GetTDP());
                            break;
                        case Function.CurrentGame:
                            response.Add(nameof(Value), systemManager.RunningGame.ToString());
                            break;
                    }

                    await args.Request.SendResponseAsync(response);
                    break;
                case Command.Set:
                    switch (function)
                    {
                        case Function.OSD:
                            var osdLevel = (int)args.Request.Message[nameof(Value)];
                            rtssManager.OSDLevel = osdLevel;
                            break;
                        case Function.TDP:
                            var tdpLimit = (int)args.Request.Message[nameof(Value)];
                            performanceManager.SetTDP(tdpLimit);
                            break;
                        case Function.GameProfile:
                            var isPerGameProfile = (bool)args.Request.Message[nameof(Value)];
                            if (systemManager.RunningGame.IsValid())
                            {
                                ProfileManager.SaveGameProfile(new GameProfile(systemManager.RunningGame.Name, systemManager.RunningGame.Path, isPerGameProfile, performanceManager.GetTDP()));
                            }
                            else
                            {
                                Logger.Info($"Running game is invalid.");
                            }
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles the event when the app service connection is closed
        /// </summary>
        private static void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            // connection to the UWP lost, so we shut down the desktop process
            //fix later
            //Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            //{
            //    Application.Current.Shutdown();
            //}));
        }
    }
}
