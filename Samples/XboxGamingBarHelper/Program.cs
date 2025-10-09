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
using XboxGamingBarHelper.Systems;

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
        private static List<IManager> Managers;

        // Properties
        private static OSDProperty osd;
        private static TDPProperty tdp;
        private static RunningGameProperty runningGame;

        static async Task Main(string[] args)
        {
            await Initialize();
        }

        private static void OnProfileChanged(object sender, ProfileChangedEventArgs e)
        {
            if (e.NewProfile.Use)
            {
                Logger.Info($"Use profile {e.NewProfile.GameId.Name}'s TDP limit {e.NewProfile.TDP}");
                tdp.Value = e.NewProfile.TDP;
            }
            else
            {
                Logger.Info($"Not using profile {e.NewProfile.GameId.Name}'s TDP limit {e.NewProfile.TDP}");
            }
        }

        private static void OnRunningGameChanged(object sender, RunningGameChangedEventArgs e)
        {
            runningGame.Value = e.NewRunningGame;
            ProfileManager.TryLoadGameProfile(e.NewRunningGame.GameId, out GameProfile gameProfile);
            profileManager.CurrentProfile = gameProfile;
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
            performanceManager = new PerformanceManager(connection);
            rtssManager = new RTSSManager(performanceManager, connection);
            profileManager = new ProfileManager(connection);
            systemManager = new SystemManager(connection);
            Managers = new List<IManager> { performanceManager, rtssManager, profileManager, systemManager };

            // Initialize properties.
            runningGame = new RunningGameProperty(systemManager.RunningGame, systemManager);
            osd = new OSDProperty(rtssManager.osdLevel, runningGame, rtssManager);
            tdp = new TDPProperty(performanceManager.GetTDP(), runningGame, performanceManager);

            AppServiceConnectionStatus status = await connection.OpenAsync();
            if (status != AppServiceConnectionStatus.Success)
            {
                return;
            }

            profileManager.ProfileChanged += OnProfileChanged;
            systemManager.RunningGameChanged += OnRunningGameChanged;

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
                            response.Add(nameof(Content), tdp.Value);
                            break;
                        case Function.CurrentGame:
                            response.Add(nameof(Content), runningGame.ToString());
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
                            tdp.Value = (int)args.Request.Message[nameof(Content)];
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
