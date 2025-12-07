using Shared.Data;
using Shared.Enums;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class ResolutionProperty : WidgetComboBoxSelectionProperty<Resolution>
    {
        public ResolutionProperty(ComboBox inUI, Page inOwner) : base(new Resolution(100, 100), Function.Resolution, inUI, inOwner)
        {
            
        }
    }
}
