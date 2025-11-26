using System;
using Shared.Enums;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class MinTDPProperty : WidgetControlProperty<int, Slider>
    {
        public MinTDPProperty(Slider inUI, Page inOwner) : base(4, Function.MinTDP, inUI, inOwner)
        {
        }

        protected override void SetControlEnabled(bool isEnabled)
        {
            // This shouldn't change the enabled state of the control.
            // base.SetControlEnabled(isEnabled);
        }

        protected override async void NotifyPropertyChanged(string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            Logger.Debug($"MaxTDPProperty changed to {Value}W");
            if (UI != null && Owner != null)
            {
                await Owner.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    UI.Minimum = Value;
                });
            }
        }
    }
}
