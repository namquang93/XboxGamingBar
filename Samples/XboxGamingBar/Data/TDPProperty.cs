using Shared.Enums;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class TDPProperty : WidgetProperty<int, Slider>
    {
        public TDPProperty(int inValue, Slider slider) : base(inValue, Function.TDP, slider)
        {
        }

        protected override void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            if (Control !=  null)
            {
                Control.Value = Value;
            }
        }
    }
}
