using Shared.Enums;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class TargetFPSProperty : WidgetSliderProperty
    {
        public TargetFPSProperty(int inValue, Slider inControl, Page inOwner) : base(inValue, Function.TargetFPS, inControl, inOwner)
        {
        }
    }
}
