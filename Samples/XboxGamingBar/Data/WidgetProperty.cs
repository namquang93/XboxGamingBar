using System;
using Shared.Data;
using Shared.Enums;
using System.Threading.Tasks;

namespace XboxGamingBar.Data
{
    internal class WidgetProperty<ValueType> : GenericProperty<ValueType, WidgetValueSet, WidgetAppServiceResponse>
    {
        public WidgetProperty(ValueType inValue, IProperty inParentProperty, Function inFunction) : base(inValue, inParentProperty, inFunction)
        {
        }

        protected override Task<WidgetAppServiceResponse> SendMessageAsync(WidgetValueSet request)
        {
            if (App.Connection == null)
            {
                Logger.Warn($"Widget property {function} doesn't have connection.");
                return null;
            }

            Logger.Info($"Sending message for widget property {function}: {request.ToDebugString()}.");
            return App.Connection.SendMessageAsync(request.ValueSet).AsTask().ContinueWith(antecedentTask =>
            {
                return new WidgetAppServiceResponse(antecedentTask.Result);
            }, TaskScheduler.Default);
        }
    }
}
