using Shared.Data;
using Shared.Utilities;
using System.ComponentModel;
using System.IO;
using Windows.ApplicationModel.AppService;
using Windows.Storage;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Settings
{
    internal class SettingsManager : Manager
    {
        private static SettingsManager instance;
        public static SettingsManager CreateInstance(AppServiceConnection connection)
        {
            if (instance == null)
            {
                instance = new SettingsManager(connection);
            }
            return instance;
        }

        public static SettingsManager GetInstance()
        {
            return instance;
        }

        private readonly OnScreenDisplayProviderProperty onScreenDisplayProvider;
        public OnScreenDisplayProviderProperty OnScreenDisplayProvider
        {
            get { return onScreenDisplayProvider; }
        }

        private readonly OnScreenDisplayProviderInstalledProperty onScreenDisplayProviderInstalled;
        public OnScreenDisplayProviderInstalledProperty OnScreenDisplayProviderInstalled
        {
            get { return onScreenDisplayProviderInstalled; }
        }

        private readonly IsForegroundProperty isForeground;
        public IsForegroundProperty IsForeground
        {
            get { return isForeground; }
        }

        private readonly string settingsPath;
        private Setting setting;
        public Setting Setting
        {
            get { return setting; }
        }
        private GenericProperty<int> onScreenDisplayPropertySettings;

        protected SettingsManager(AppServiceConnection connection) : base(connection)
        {
            settingsPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "settings.xml");
            if (File.Exists(settingsPath))
            {
                Logger.Info("Loading settings from XML file.");
                setting = XmlHelper.FromXMLFile<Setting>(settingsPath);
            }
            else
            {
                Logger.Info("Settings XML file not found, using default settings.");
                setting = new Setting(0, 0);
                XmlHelper.ToXMLFile(setting, settingsPath);
            }

            onScreenDisplayProvider = new OnScreenDisplayProviderProperty(setting.OnScreenDisplayProvider, this);
            onScreenDisplayProvider.PropertyChanged += OnScreenDisplayProviderChanged;
            onScreenDisplayProviderInstalled = new OnScreenDisplayProviderInstalledProperty(this);
            isForeground = new IsForegroundProperty(this);
        }

        public void SyncOnScreenDisplaySettings(GenericProperty<int> onScreenDisplayProperty)
        {
            if (onScreenDisplayPropertySettings != null)
                onScreenDisplayPropertySettings.PropertyChanged -= PerformanceOverlayLevelChanged;
            onScreenDisplayPropertySettings = onScreenDisplayProperty;
            onScreenDisplayProperty.PropertyChanged += PerformanceOverlayLevelChanged;
        }

        private void OnScreenDisplayProviderChanged(object sender, PropertyChangedEventArgs e)
        {
            Logger.Info($"Save setting On-Screen Display Provider {onScreenDisplayProvider.Value}.");

            setting.OnScreenDisplayProvider = OnScreenDisplayProvider.Value;
            XmlHelper.ToXMLFile(setting, settingsPath);
        }

        public void PerformanceOverlayLevelChanged(object sender, PropertyChangedEventArgs e)
        {
            if (onScreenDisplayPropertySettings == null)
            {
                Logger.Error($"On-Screen Display Property for settings not found.");
                return;
            }
            else
            {
                Logger.Info($"Save setting On-Screen Display {onScreenDisplayPropertySettings.Value}.");

                setting.OnScreenDisplay = onScreenDisplayPropertySettings.Value;
                XmlHelper.ToXMLFile(setting, settingsPath);
            }
        }

        public override void Update()
        {
            base.Update();

            Logger.Debug($"On-Screen Display provider {Program.onScreenDisplay.Manager.GetType().Name} is {(Program.onScreenDisplay.Manager.IsInstalled ? "installed" : "not installed")}.");
            onScreenDisplayProviderInstalled.SetValue(Program.onScreenDisplay.Manager.IsInstalled);
        }
    }
}
