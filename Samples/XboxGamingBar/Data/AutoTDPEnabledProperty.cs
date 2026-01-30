using Shared.Enums;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class AutoTDPEnabledProperty : WidgetToggleProperty
    {
        public AutoTDPEnabledProperty(bool inValue, ToggleSwitch inUI, Page inOwner) : base(inValue, Function.AutoTDPEnabled, inUI, inOwner)
        {
        }
    }
}
