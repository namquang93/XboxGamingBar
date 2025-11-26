using System;
using Shared.Enums;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class MaxTDPProperty : WidgetControlProperty<int, Slider>
    {
        public MaxTDPProperty(Slider inUI, Page inOwner) : base(32, Function.MaxTDP, inUI, inOwner)
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
                    UI.Maximum = Value;
                });
            }
        }
    }
}
