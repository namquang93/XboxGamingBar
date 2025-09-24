using Microsoft.Win32;
using RTSSSharedMemoryNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
// using System.Windows.Controls;

namespace XboxGamingBarHelper
{
    internal class Program
    {
        private AppServiceConnection connection = null;
        static AutoResetEvent done = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            // Console.Title = "Xbox Gaming Bar Helper";
            // Console.WriteLine($"OSD {OSD.GetOSDEntries().Length} APP {OSD.GetAppEntries().Length}");
            // Console.WriteLine("\r\nPress any key to exit ...");
            // Console.ReadLine();

            // await InitializeAppServiceConnection();

            Thread bgThread = new Thread(ThreadProc);
            bgThread.Start(done);
            done.WaitOne();
        }

        static void ThreadProc(object unused)
        {
            // keep this up ofr 60sec, just for demo purposes
            Thread.Sleep(60000);
            done.Set();
        }

        /// <summary>
        /// Open connection to UWP app service
        /// </summary>
        private async void InitializeAppServiceConnection()
        {
            connection = new AppServiceConnection();
            connection.AppServiceName = "SampleInteropService";
            connection.PackageFamilyName = Package.Current.Id.FamilyName;
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;

            AppServiceConnectionStatus status = await connection.OpenAsync();
            if (status != AppServiceConnectionStatus.Success)
            {
                // something went wrong ...
                // MessageBox.Show(status.ToString());
                // this.IsEnabled = false;
            }
        }

        /// <summary>
        /// Handles the event when the desktop process receives a request from the UWP app
        /// </summary>
        private async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // retrive the reg key name from the ValueSet in the request
            string key = args.Request.Message["KEY"] as string;
            int index = key.IndexOf('\\');
            if (index > 0)
            {
                // read the key values from the respective hive in the registry
                string hiveName = key.Substring(0, key.IndexOf('\\'));
                string keyName = key.Substring(key.IndexOf('\\') + 1);
                RegistryHive hive = RegistryHive.ClassesRoot;

                switch (hiveName)
                {
                    case "HKLM":
                        hive = RegistryHive.LocalMachine;
                        break;
                    case "HKCU":
                        hive = RegistryHive.CurrentUser;
                        break;
                    case "HKCR":
                        hive = RegistryHive.ClassesRoot;
                        break;
                    case "HKU":
                        hive = RegistryHive.Users;
                        break;
                    case "HKCC":
                        hive = RegistryHive.CurrentConfig;
                        break;
                }

                using (RegistryKey regKey = RegistryKey.OpenRemoteBaseKey(hive, "").OpenSubKey(keyName))
                {
                    // compose the response as ValueSet
                    ValueSet response = new ValueSet();
                    if (regKey != null)
                    {
                        foreach (string valueName in regKey.GetValueNames())
                        {
                            response.Add(valueName, regKey.GetValue(valueName).ToString());
                        }
                    }
                    else
                    {
                        response.Add("ERROR", "KEY NOT FOUND");
                    }
                    // send the response back to the UWP
                    await args.Request.SendResponseAsync(response);
                }
            }
            else
            {
                ValueSet response = new ValueSet();
                response.Add("ERROR", "INVALID REQUEST");
                await args.Request.SendResponseAsync(response);
            }
        }

        /// <summary>
        /// Handles the event when the app service connection is closed
        /// </summary>
        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
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
