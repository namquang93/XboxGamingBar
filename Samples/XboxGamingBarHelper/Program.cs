using NLog;
using Shared.Data;
using Shared.Enums;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using XboxGamingBarHelper.Performance;
using XboxGamingBarHelper.RTSS;
using XboxGamingBarHelper.System;

namespace XboxGamingBarHelper
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static AppServiceConnection connection = null;

        private static bool needToUpdate = false;

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
            PerformanceManager.Initialize();
            RTSSManager.Initialize();
            SystemManager.RunningGameChanged += OnRunningGameChanged;
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
                needToUpdate = false;
                return;
            }

            needToUpdate = true;
            while (needToUpdate)
            {
                await Task.Delay(500);

                PerformanceManager.Update();
                RTSSManager.Update();
                SystemManager.Update();
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
                            response.Add(nameof(Value), RTSSManager.OSDLevel);
                            break;
                        case Function.TDP:
                            response.Add(nameof(Value), PerformanceManager.GetTDP());
                            break;
                        case Function.CurrentGame:
                            response.Add(nameof(Value), SystemManager.RunningGame.ToString());
                            break;
                    }

                    await args.Request.SendResponseAsync(response);
                    break;
                case Command.Set:
                    switch (function)
                    {
                        case Function.OSD:
                            var osdLevel = (int)args.Request.Message[nameof(Value)];
                            RTSSManager.OSDLevel = osdLevel;
                            break;
                        case Function.TDP:
                            var tdpLimit = (int)args.Request.Message[nameof(Value)];
                            PerformanceManager.SetTDP(tdpLimit);
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
