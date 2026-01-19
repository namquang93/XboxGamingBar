using Shared.Enums;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class OnScreenDisplayProviderInstalledProperty : WidgetControlVisibleProperty<FrameworkElement>
    {
        public OnScreenDisplayProviderInstalledProperty(FrameworkElement inUI, Page inOwner, params FrameworkElement[] inAdditionalUIs)
            : base(true, Function.Settings_OnScreenDisplayProviderInstalled, inUI, inOwner, inAdditionalUIs)
        {
        }
    }
}