using Shared.Enums;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class AMDAntiLagEnabledProperty : WidgetToggleProperty
    {
        public AMDAntiLagEnabledProperty(ToggleSwitch inUI, Page inOwner) : base(false, Function.AMDAntiLagEnabled, inUI, inOwner)
        {
        }
    }
}
