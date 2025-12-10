using Shared.Enums;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class FocusingOnOSDSliderProperty : WidgetControlFocusProperty<Slider>
    {
        public FocusingOnOSDSliderProperty(Slider inUI, Page inOwner) : base(false, Function.FocusingOnOSDSlider, inUI, inOwner)
        {
        }

        // This should be controlled by the widget only, so never get value from helper.
        public override bool ShouldSync()
        {
            //return base.ShouldSync();
            return false;
        }
    }
}
