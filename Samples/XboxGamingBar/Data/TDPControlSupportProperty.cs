using Shared.Enums;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class TDPControlSupportProperty : WidgetControlVisibleProperty<UIElement>
    {
        public TDPControlSupportProperty(UIElement inUI, Page inOwner, params UIElement[] inAdditionalUIs) : base(false, Function.Support_TDPControl, inUI, inOwner, inAdditionalUIs)
        {
        }
    }
}
