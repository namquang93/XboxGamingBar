using System;
using Shared.Data;
using Shared.Enums;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls;

namespace XboxGamingBar.Data
{
    internal class WidgetProperty<ValueType, ControlType> : GenericProperty<ValueType> where ControlType : Control
    {
        private AppServiceConnection connection;
        public AppServiceConnection Connection
        {
            get { return connection; }
            set
            {
                connection = value;
            }
        }

        private ControlType control;
        public ControlType Control
        {
            get { return control; }
        }

        public WidgetProperty(ValueType inValue, Function inFunction, ControlType inControl) : base(inValue, null, inFunction)
        {
            control = inControl;
        }

        protected override Task<AppServiceResponse> SendMessageAsync(ValueSet request)
        {
            if (connection == null)
            {
                Logger.Warn($"Widget property {function} doesn't have connection.");
                return null;
            }

            return connection.SendMessageAsync(request).AsTask();
        }

        public override async Task SyncProperty()
        {
            if (Control != null)
            {
                Control.IsEnabled = false;
            }

            await base.SyncProperty();

            if (Control != null)
            {
                Control.IsEnabled = true;
            }
        }
    }
}
