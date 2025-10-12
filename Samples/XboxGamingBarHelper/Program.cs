using NLog;
using Shared.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
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
            connection = new AppServiceConnection();
            connection.AppServiceName = "XboxGamingBarService";
            connection.PackageFamilyName = Package.Current.Id.FamilyName;
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;

            //while (!System.Diagnostics.Debugger.IsAttached)
            //{
            //    await Task.Delay(500);
            //}

            // Initialize managers.
            performanceManager = new PerformanceManager(connection);
            rtssManager = new RTSSManager(performanceManager, connection);
            profileManager = new ProfileManager(connection);
            systemManager = new SystemManager(connection);
            Managers = new List<IManager> { performanceManager, rtssManager, profileManager, systemManager };

            // Initialize properties.
            properties = new HelperProperties(systemManager.RunningGame, rtssManager.OSD, performanceManager.TDP, profileManager.PerGameProfile);

            systemManager.RunningGame.PropertyChanged += RunningGame_PropertyChanged;
            profileManager.PerGameProfile.PropertyChanged += PerGameProfile_PropertyChanged;
            performanceManager.TDP.PropertyChanged += TDP_PropertyChanged;

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

        private static void PerGameProfile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //profileManager.CurrentProfile.Use = profileManager.PerGameProfile;
        }

        private static void TDP_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Logger.Info($"Set current profile {profileManager.CurrentProfile.GameId.Name}'s TDP from {profileManager.CurrentProfile.TDP} to {performanceManager.TDP}");
            profileManager.CurrentProfile.TDP = performanceManager.TDP;
        }

        private static void RunningGame_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            
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
            // connection to the UWP lost, so we shut down the desktop process
            //fix later
            //Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            //{
            //    Application.Current.Shutdown();
            //}));
        }
    }
}
