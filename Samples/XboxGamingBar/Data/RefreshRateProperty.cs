using Shared.Constants;
using Shared.Enums;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class RefreshRateProperty : WidgetComboBoxSelectionProperty<int>
    {
        public RefreshRateProperty(ComboBox inUI, Page inOwner) : base(SystemConstants.DEFAULT_REFRESH_RATE, Function.RefreshRate, inUI, inOwner)
        {
            
        }
    }
}
