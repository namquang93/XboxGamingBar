using Shared.Enums;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class AMDAntiLagSupportedProperty : WidgetControlEnabledProperty<ToggleSwitch>
    {
        public AMDAntiLagSupportedProperty(ToggleSwitch inUI, Page inOwner) : base(Function.AMDAntiLagSupported, inUI, inOwner)
        {
        }
    }
}
