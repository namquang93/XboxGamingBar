using Shared.Enums;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Settings
{
    internal class IsListeningForKeyBindingProperty : HelperProperty<bool, SettingsManager>
    {
        public IsListeningForKeyBindingProperty(SettingsManager inManager) : base(false, null, Function.IsListeningForKeyBinding, inManager)
        {
        }
    }
}
