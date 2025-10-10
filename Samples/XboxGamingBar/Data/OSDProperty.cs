using System;
using Shared.Enums;
using System.Runtime.CompilerServices;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class OSDProperty : WidgetProperty<int, Slider>
    {
        public OSDProperty(int inValue, Slider slider, Page owner) : base(inValue, Function.OSD, slider, owner)
        {
        }

        protected override async void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            if (Control != null && Owner != null)
            {
                await Owner.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { Control.Value = Value; });
            }
        }
    }
}
