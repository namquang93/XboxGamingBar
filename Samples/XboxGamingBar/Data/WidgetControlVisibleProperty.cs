using System;
using Shared.Enums;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    // A property that changes the visibility of a widget control
    internal class WidgetControlVisibleProperty<UIType> : WidgetUIProperty<bool, UIType> where UIType : UIElement
    {
        private UIType[] additionalUIs;
        public UIType[] AdditionalUIs
        {
            get { return additionalUIs; }
        }

        public WidgetControlVisibleProperty(bool inValue, Function inFunction, UIType inUI, Page inOwner, params UIType[] inAdditionalUIs) : base(inValue, inFunction, inUI, inOwner)
        {
            additionalUIs = inAdditionalUIs;
        }

        protected override async void NotifyPropertyChanged(string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            if (UI != null && Owner != null)
            {
                await Owner.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Logger.Debug($"{(Value ? "Show" : "Hide")} {UI} property changed.");
                    UI.Visibility = Value ? Visibility.Visible : Visibility.Collapsed;

                    foreach (var additionalUI in AdditionalUIs)
                    {
                        if (additionalUI != null)
                        {
                            Logger.Debug($"{(Value ? "Show" : "Hide")} {additionalUI} property changed.");
                            additionalUI.Visibility = Value ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                });
            }
        }
    }
}
