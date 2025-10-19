using Shared.Enums;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class PerGameProfileProperty : WidgetToggleProperty
    {
        public PerGameProfileProperty(ToggleSwitch inUI, Page inOwner) : base(false, Function.PerGameProfile, inUI, inOwner)
        {
        }
    }
}
