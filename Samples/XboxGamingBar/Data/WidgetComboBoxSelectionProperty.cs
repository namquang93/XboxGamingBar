using Shared.Enums;
using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class WidgetComboBoxSelectionProperty<ValueType> : WidgetControlProperty<ValueType, ComboBox> where ValueType : IEquatable<ValueType>
    {
        public WidgetComboBoxSelectionProperty(ValueType inValue, Function inFunction, ComboBox inUI, Page inOwner) : base(inValue, inFunction, inUI, inOwner)
        {
            if (UI != null)
            {
                UI.SelectionChanged += ComboBox_SelectionChanged;
            }
        }

        protected virtual void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ValueType selectedValue && !selectedValue.Equals(Value))
            {
                Logger.Info($"{Function} combo box updated to {selectedValue}.");
                SetValue(selectedValue);
            }
            else
            {
                Logger.Info($"{Function} combo box changed to {(e.AddedItems.Count > 0 ? e.AddedItems[0] : "none")}.");
            }
        }

        protected override async void NotifyPropertyChanged(string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            if (UI != null && Owner != null)
            {
                // Logger.Info($"Update {Function} combo box value {Value}.");
                await Owner.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    for (var i = 0; i < UI.Items.Count; i++)
                    {
                        if (UI.Items[i] is ValueType selectedValue && selectedValue.Equals(Value))
                        {
                            Logger.Info($"{Function} combo box selected index {i}.");
                            UI.SelectedIndex = i;
                            break;
                        }
                    }
                });
            }
        }
    }
}
