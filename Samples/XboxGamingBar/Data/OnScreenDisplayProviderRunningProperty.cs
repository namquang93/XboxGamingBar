using Shared.Enums;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class OnScreenDisplayProviderRunningProperty : WidgetControlEnabledProperty<Slider>
    {
        public OnScreenDisplayProviderRunningProperty(Slider inUI, Page inOwner) : base(Function.Settings_OnScreenDisplayProviderRunning, inUI, inOwner)
        {
        }
    }
}
