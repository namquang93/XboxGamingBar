using Shared.Enums;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Settings
{
    internal class OnScreenDisplayProviderProperty : HelperProperty<int, SettingsManager>
    {
        public OnScreenDisplayProviderProperty(SettingsManager inManager) : base(0, null, Function.Settings_OnScreenDisplayProvider, inManager)
        {
        }
    }
}
