using Windows.ApplicationModel.AppService;
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

        private readonly AutoStartRTSSProperty autoStartRTSS;
        public AutoStartRTSSProperty AutoStartRTSS => autoStartRTSS;

        protected SettingsManager(AppServiceConnection connection) : base(connection)
        {
            autoStartRTSS = new AutoStartRTSSProperty(this);
        }
    }
}
