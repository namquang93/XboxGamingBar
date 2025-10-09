using Shared.Enums;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class OSDProperty : WidgetProperty<int, Slider>
    {
        public OSDProperty(int inValue, Slider slider) : base(inValue, Function.OSD, slider)
        {
        }

        protected override void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            if (Control != null)
            {
                Control.Value = Value;
            }
        }
    }
}
