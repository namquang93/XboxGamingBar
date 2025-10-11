using Shared.Data;
using Shared.Enums;
using System;
using System.Runtime.CompilerServices;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class RunningGameProperty : WidgetProperty<RunningGame, TextBlock>
    {
        public RunningGameProperty(TextBlock inControl, Page inOwner) : base(new RunningGame(), Function.CurrentGame, inControl, inOwner)
        {
        }

        public override bool SetValue(object value)
        {
            if (value is string stringValue)
            {
                return base.SetValue(RunningGame.FromString(stringValue));
            }
            else
            {
                return base.SetValue(value);
            }
        }

        public override ValueSet AddValueSetContent(in ValueSet inValueSet)
        {
            inValueSet.Add(nameof(Content), Value.ToString());
            return inValueSet;
        }

        protected override async void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            if (UI != null && Owner != null)
            {
                Logger.Info($"Update TDP slider value {Value}.");
                await Owner.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { UI.Text = Value.IsValid() ? Value.GameId.Name : "No Game Detected"; });
            }
        }
    }
}
