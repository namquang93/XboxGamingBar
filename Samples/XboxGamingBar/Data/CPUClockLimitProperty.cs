using Shared.Enums;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class CPUClockLimitProperty : WidgetToggleProperty
    {
        public CPUClockLimitProperty(ToggleSwitch inUI, Page inOwner) : base(false, Function.CPUClockLimit, inUI, inOwner)
        {
        }
    }
}
