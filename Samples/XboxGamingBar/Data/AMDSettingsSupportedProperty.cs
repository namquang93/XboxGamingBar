using Shared.Enums;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class AMDSettingsSupportedProperty : WidgetControlVisibleProperty<UIElement>
    {
        public AMDSettingsSupportedProperty(bool inValue, UIElement inUI, Page inOwner, params UIElement[] inAdditionalUIs) : base(inValue, Function.Support_AMDSettings, inUI, inOwner, inAdditionalUIs)
        {
        }
    }
}
