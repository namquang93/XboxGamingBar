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

        // Managers
        private static PerformanceManager performanceManager;
        private static RTSSManager rtssManager;
        private static ProfileManager profileManager;
        private static SystemManager systemManager;
        private static List<IManager> Managers = new List<IManager>();

        // Properties
        private static OSDProperty osd;
        private static TDPProperty tdp;
        private static RunningGameProperty runningGame;

        static async Task Main(string[] args)
        {
            await Initialize();
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
                { nameof(Content), e.NewRunningGame.ToString() }
            };
            AppServiceResponse response = await connection.SendMessageAsync(request);
            if (response.Message.TryGetValue(nameof(Content), out var result))
            {
                Logger.Info($"Update running game {(string)result}.");
            }
            else
            {
                Logger.Info("Can't update current game.");
            }

            if (!ProfileManager.TryLoadGameProfile(e.NewRunningGame.GameId, out GameProfile gameProfile))
            {
                gameProfile = profileManager.GlobalProfile;
                Logger.Info($"New running game {e.NewRunningGame.GameId.Name} doesn't have profile, use global profile.");
            }

            profileManager.CurrentProfile = gameProfile;
            if (gameProfile.Use)
            {
                Logger.Info($"New running game {e.NewRunningGame.GameId.Name} have profile in used, apply it.");
                request = new ValueSet
                    {
                        { nameof(Command), (int)Command.Update },
                        { nameof(Function),(int)Function.TDP },
                        { nameof(Content), gameProfile.TDP }
                    };
                response = await connection.SendMessageAsync(request);
                if (response.Message.TryGetValue(nameof(Content), out result))
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
                Logger.Info($"New running game {e.NewRunningGame.GameId.Name} have profile in used, do not apply it.");
            }
        }

        /// <summary>
        /// Open connection to UWP app service
        /// </summary>
        private static async Task Initialize()
        {
            // Initialize app service connection.
            connection = new AppServiceConnection();
            connection.AppServiceName = "XboxGamingBarService";
            connection.PackageFamilyName = Package.Current.Id.FamilyName;
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;

            // Initialize managers.
            performanceManager = new PerformanceManager();
            Managers.Add(performanceManager);

            rtssManager = new RTSSManager(performanceManager);
            Managers.Add(rtssManager);

            profileManager = new ProfileManager();
            Managers.Add(profileManager);

            systemManager = new SystemManager();
            Managers.Add(systemManager);
            systemManager.RunningGameChanged += OnRunningGameChanged;

            // Initialize properties.
            runningGame = new RunningGameProperty(systemManager.RunningGame, connection, Function.CurrentGame);
            osd = new OSDProperty(rtssManager.osdLevel, runningGame, connection, Function.OSD);

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
                            response.Add(nameof(Content), rtssManager.osdLevel);
                            break;
                        case Function.TDP:
                            response.Add(nameof(Content), performanceManager.GetTDP());
                            break;
                        case Function.CurrentGame:
                            response.Add(nameof(Content), systemManager.RunningGame.ToString());
                            break;
                    }

                    await args.Request.SendResponseAsync(response);
                    break;
                case Command.Set:
                    switch (function)
                    {
                        case Function.OSD:
                            var osdLevel = (int)args.Request.Message[nameof(Content)];
                            rtssManager.osdLevel = osdLevel;
                            break;
                        case Function.TDP:
                            var tdpLimit = (int)args.Request.Message[nameof(Content)];
                            performanceManager.SetTDP(tdpLimit);
                            break;
                        case Function.GameProfile:
                            var isPerGameProfile = (bool)args.Request.Message[nameof(Content)];
                            if (systemManager.RunningGame.IsValid())
                            {
                                ProfileManager.SaveGameProfile(new GameProfile(systemManager.RunningGame.GameId, isPerGameProfile, performanceManager.GetTDP()));
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
