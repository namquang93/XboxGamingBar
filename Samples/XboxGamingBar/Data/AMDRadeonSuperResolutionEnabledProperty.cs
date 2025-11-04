using Shared.Enums;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class AMDRadeonSuperResolutionEnabledProperty : WidgetControlProperty<bool, ToggleSwitch>
    {
        public AMDRadeonSuperResolutionEnabledProperty(ToggleSwitch inUI, Page inOwner) : base(false, Function.AMDRadeonSuperResolutionEnabled, inUI, inOwner)
        {
        }
    }
}
