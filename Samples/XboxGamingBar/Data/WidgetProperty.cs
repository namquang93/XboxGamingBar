using Shared.Data;
using Shared.Enums;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class WidgetProperty<ValueType, ControlType> : GenericProperty<ValueType> where ControlType : Control
    {
        private ControlType control;
        public ControlType Control
        {
            get { return control; }
        }

        private readonly Page owner;
        public Page Owner { get { return owner; } }

        public WidgetProperty(ValueType inValue, Function inFunction, ControlType inControl, Page inOwner) : base(inValue, null, inFunction)
        {
            control = inControl;
            owner = inOwner;
        }

        protected override Task<AppServiceResponse> SendMessageAsync(ValueSet request)
        {
            if (App.Connection == null)
            {
                Logger.Warn($"Widget property {function} doesn't have connection.");
                return null;
            }

            return App.Connection.SendMessageAsync(request).AsTask();
        }

        public override async Task Sync()
        {
            if (Control != null && Owner != null)
            {
                await Owner.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { Control.IsEnabled = false; });
            }

            await base.Sync();

            if (Control != null && Owner != null)
            {
                await Owner.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { Control.IsEnabled = true; });
            }
        }
    }
}
