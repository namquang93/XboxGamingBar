using System;
using Shared.Enums;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class LosslessScalingShortcutProperty : WidgetProperty<List<int>>
    {
        private readonly Button button;

        public LosslessScalingShortcutProperty(Button inUI, List<int> inValue) : base(inValue, null, Function.LosslessScalingShortcut)
        {
            button = inUI;
        }

        public void RefreshUI()
        {
            NotifyPropertyChanged();
        }

        protected override async void NotifyPropertyChanged(string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            if (button != null)
            {
                await button.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (Value == null || Value.Count == 0)
                    {
                        button.Content = "None";
                    }
                    else
                    {
                        button.Content = string.Join(" + ", Value.Select(k => FormatGamepadKey((VirtualKey)k)));
                    }
                });
            }
        }

        public static string FormatGamepadKey(VirtualKey key)
        {
            string name = key.ToString();
            if (name.StartsWith("Gamepad"))
            {
                name = name.Substring(7);
            }

            switch (name)
            {
                case "RightThumbstickButton": return "RTB";
                case "LeftThumbstickButton": return "LTB";
                case "LeftShoulder": return "LB";
                case "RightShoulder": return "RB";
                case "LeftTrigger": return "LT";
                case "RightTrigger": return "RT";
                case "DPadUp": return "↑";
                case "DPadDown": return "↓";
                case "DPadLeft": return "←";
                case "DPadRight": return "→";
                case "RightThumbstickLeft": return "←R";
                case "RightThumbstickRight": return "R→";
                case "RightThumbstickUp": return "↑R";
                case "RightThumbstickDown": return "R↓";
                default: return name;
            }
        }
    }
}
