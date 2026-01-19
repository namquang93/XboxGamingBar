using Shared.Enums;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Settings
{
    internal class OnScreenDisplayProviderRunningProperty : HelperProperty<bool, SettingsManager>
    {
        public OnScreenDisplayProviderRunningProperty(SettingsManager inManager) : base(false, null, Function.Settings_OnScreenDisplayProviderRunning, inManager)
        {

        }
    }
}
