using System;
using Shared.Enums;
using System.Runtime.CompilerServices;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace XboxGamingBar.Data
{
    internal class TDPProperty : WidgetProperty<int, Slider>
    {
        public TDPProperty(int inValue, Slider slider, Page owner) : base(inValue, Function.TDP, slider, owner)
        {
            if (Control != null)
            {
                Control.ValueChanged += Slider_ValueChanged;
            }
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Logger.Info($"TDP Slider value changed from {e.OldValue} to {e.NewValue}, update property.");
            Value = (int)e.NewValue;
        }

        protected override async void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            if (Control != null && Owner != null)
            {
                Logger.Info($"Update TDP slider value {Value}.");
                await Owner.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { Control.Value = Value; });
            }
        }
    }
}
