using Shared.Data;
using Shared.Enums;
using Shared.Utilities;
using System;
using System.Runtime.CompilerServices;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class RunningGameProperty : WidgetPropertyWithAdditionalUI<RunningGame, TextBlock, ToggleSwitch>
    {
        public RunningGameProperty(TextBlock inUI, ToggleSwitch inAdditionalUI, Page inOwner) : base(new RunningGame(), Function.CurrentGame, inUI, inAdditionalUI, inOwner)
        {
        }

        public override bool SetValue(object value)
        {
            if (value is string stringValue)
            {
                return base.SetValue(XmlHelper.FromXMLString<RunningGame>(stringValue));
            }
            else
            {
                return base.SetValue(value);
            }
        }

        public override ValueSet AddValueSetContent(in ValueSet inValueSet)
        {
            inValueSet.Add(nameof(Content), XmlHelper.ToXMLString(Value, true));
            return inValueSet;
        }

        protected override async void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            if (UI != null && Owner != null)
            {
                Logger.Info($"Update running game value {Value.GameId.Name}.");
                await Owner.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    UI.Text = Value.IsValid() ? Value.GameId.Name : "No Game Detected";
                    AdditionalUI.Visibility = Value.IsValid() ? Visibility.Visible : Visibility.Collapsed;
                });
            }
        }
    }
}
