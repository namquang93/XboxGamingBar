﻿using Shared.Enums;
using System;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class RefreshRatesProperty : WidgetControlProperty<List<int>, ComboBox>
    {
        public RefreshRatesProperty(ComboBox inUI, Page inOwner) : base(new List<int>() { 60 }, Function.RefreshRates, inUI, inOwner)
        {
        }

        protected override async void NotifyPropertyChanged(string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            if (UI != null && Owner != null)
            {
                Logger.Info($"Update {Function} slider value {Value}.");
                await Owner.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    UI.Items.Clear();
                    foreach (var value in Value)
                    {
                        UI.Items.Add(value);
                    }
                });
            }
        }
    }
}
