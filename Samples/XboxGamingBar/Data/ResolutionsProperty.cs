using System;
using Shared.Data;
using Shared.Enums;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class ResolutionsProperty : WidgetControlProperty<Resolutions, ComboBox>
    {
        public ResolutionsProperty(ComboBox inUI, Page inOwner) : base(new Resolutions(), Function.Resolutions, inUI, inOwner)
        {
        }

        protected override async void NotifyPropertyChanged(string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            if (UI != null && Owner != null)
            {
                Logger.Info($"Update {Function} combo box value {Value}.");
                await Owner.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    UI.Items.Clear();
                    foreach (var value in Value.AvailableResolutions)
                    {
                        UI.Items.Add(value);
                    }
                });
            }
        }
    }
}
