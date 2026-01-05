using Shared.Data;
using System;
using System.Threading.Tasks;

namespace XboxGamingBar.Data
{
    internal class WidgetProperties : FunctionalProperties<WidgetValueSet, WidgetAppServiceResponse, WidgetAppServiceRequest>
    {
        public WidgetProperties(params FunctionalProperty<WidgetValueSet, WidgetAppServiceResponse>[] inProperties) : base(inProperties) { }

        protected override Task<SharedAppServiceResponseStatus> SendResponse(WidgetAppServiceRequest request, WidgetValueSet response)
        {
            Logger.Info($"Sending response request {request.Message.ToDebugString()}: {response.ToDebugString()}.");
            return request.SendResponseAsync(response).AsTask().ContinueWith(antecedentTask =>
            {
                return (SharedAppServiceResponseStatus)(int)antecedentTask.Result;
            }, TaskScheduler.Default);
        }

        public async Task Sync()
        {
            foreach (var property in properties)
            {
                await property.Value.Sync();
            }
        }
    }
}
