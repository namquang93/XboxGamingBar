using Shared.Data;
using Shared.Enums;
using System;
using System.Runtime.CompilerServices;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class GameProfileProperty : WidgetProperty<GameProfile, ToggleSwitch>
    {
        public GameProfileProperty(ToggleSwitch inUI, Page inOwner) : base(new GameProfile(), Function.GameProfile, inUI, inOwner)
        {

        }

        public override bool SetValue(object value)
        {
            if (value is string stringValue)
            {
                return base.SetValue(GameProfile.FromString(stringValue));
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
                await Owner.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => 
                {
                    UI.OnContent = "Per-game Profile";
                    UI.OffContent = "Global Profile";
                });
            }
        }
    }
}
