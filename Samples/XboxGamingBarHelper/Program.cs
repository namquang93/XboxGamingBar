﻿using NLog;
using Shared.Data;
using Shared.Enums;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
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
                            var serializer = new XmlSerializer(typeof(RunningGame));
                            var writer = new StringWriter();
                            serializer.Serialize(writer, SystemManager.GetRunningGame());
                            var currentGameInfo = writer.ToString();
                            Logger.Info($"Current game {currentGameInfo}");
                            response.Add(nameof(Value), currentGameInfo);
                            writer.Dispose();
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
            // retrive the reg key name from the ValueSet in the request
            //string key = args.Request.Message["KEY"] as string;
            //int index = key.IndexOf('\\');
            //if (index > 0)
            //{
            //    // read the key values from the respective hive in the registry
            //    string hiveName = key.Substring(0, key.IndexOf('\\'));
            //    string keyName = key.Substring(key.IndexOf('\\') + 1);
            //    RegistryHive hive = RegistryHive.ClassesRoot;

            //    switch (hiveName)
            //    {
            //        case "HKLM":
            //            hive = RegistryHive.LocalMachine;
            //            break;
            //        case "HKCU":
            //            hive = RegistryHive.CurrentUser;
            //            break;
            //        case "HKCR":
            //            hive = RegistryHive.ClassesRoot;
            //            break;
            //        case "HKU":
            //            hive = RegistryHive.Users;
            //            break;
            //        case "HKCC":
            //            hive = RegistryHive.CurrentConfig;
            //            break;
            //    }

            //    using (RegistryKey regKey = RegistryKey.OpenRemoteBaseKey(hive, "").OpenSubKey(keyName))
            //    {
            //        // compose the response as ValueSet
            //        ValueSet response = new ValueSet();
            //        if (regKey != null)
            //        {
            //            foreach (string valueName in regKey.GetValueNames())
            //            {
            //                response.Add(valueName, regKey.GetValue(valueName).ToString());
            //            }
            //        }
            //        else
            //        {
            //            response.Add("ERROR", "KEY NOT FOUND");
            //        }
            //        // send the response back to the UWP
            //        await args.Request.SendResponseAsync(response);
            //    }
            //}
            //else
            //{
            //    ValueSet response = new ValueSet();
            //    response.Add("ERROR", "INVALID REQUEST");
            //    await args.Request.SendResponseAsync(response);
            //}
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
