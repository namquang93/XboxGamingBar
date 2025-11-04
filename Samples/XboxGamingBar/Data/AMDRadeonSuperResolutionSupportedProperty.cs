using Shared.Enums;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class AMDRadeonSuperResolutionSupportedProperty : WidgetControlProperty<bool, ToggleSwitch>
    {
        public AMDRadeonSuperResolutionSupportedProperty(ToggleSwitch inUI, Page inOwner) : base(false, Function.AMDRadeonSuperResolutionSupported, inUI, inOwner)
        {
        }
    }
}
