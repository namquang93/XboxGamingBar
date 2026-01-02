using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using NLog;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Shared.Enums;

namespace XboxGamingBarHelper.Systems
{
    public class TrayIconManager : IDisposable
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;
        private AppServiceConnection connection;

        public TrayIconManager(AppServiceConnection connection)
        {
            this.connection = connection;
            InitializeTrayIcon();
        }

        public AppServiceConnection Connection
        {
            get => connection;
            set => connection = value;
        }

        private void InitializeTrayIcon()
        {
            contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Exit", null, OnExitClicked);

            notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application, // Fallback icon
                ContextMenuStrip = contextMenu,
                Text = "Xbox Gaming Bar Helper",
                Visible = true
            };

            // Try to load a better icon if possible
            try
            {
                string[] iconPaths = new string[]
                {
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Icons", "tray_icon_32.png"),
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Icons", "tray_icon.png"),
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tray_icon.png"),
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Images", "Square44x44Logo.png")
                };

                foreach (string iconPath in iconPaths)
                {
                    if (System.IO.File.Exists(iconPath))
                    {
                        Logger.Info($"Found tray icon at {iconPath}");
                        using (Bitmap bitmap = new Bitmap(iconPath))
                        {
                            IntPtr hIcon = bitmap.GetHicon();
                            notifyIcon.Icon = Icon.FromHandle(hIcon);
                            // Note: notifyIcon.Icon takes ownership of the icon object, 
                            // but hIcon itself is a GDI handle that can be destroyed after the Icon object is created.
                            DestroyIcon(hIcon);
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load tray icon image. Using default.");
            }
        }

        private async void OnExitClicked(object sender, EventArgs e)
        {
            Logger.Info("Exit clicked from tray icon.");
            
            if (connection != null)
            {
                try
                {
                    Logger.Info("Signaling widget to close...");
                    ValueSet message = new ValueSet();
                    message.Add("Function", (int)Function.AppExit);
                    await connection.SendMessageAsync(message);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to send AppExit message to widget.");
                }
            }

            notifyIcon.Visible = false;
            Application.Exit();
            Environment.Exit(0);
        }

        public void Dispose()
        {
            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
            }
            contextMenu?.Dispose();
        }
    }
}
