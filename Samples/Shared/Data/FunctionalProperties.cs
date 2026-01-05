using NLog;
using Shared.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared.Data
{
    public abstract class FunctionalProperties<TSharedValueSet, TSharedAppServiceResponse, TSharedAppServiceRequest>
        where TSharedAppServiceResponse : SharedAppServiceResponse
        where TSharedAppServiceRequest : SharedAppServiceRequest
        where TSharedValueSet : SharedValueSet, new()
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected readonly Dictionary<Function, FunctionalProperty<TSharedValueSet, TSharedAppServiceResponse>> properties;

        public FunctionalProperties(params FunctionalProperty<TSharedValueSet, TSharedAppServiceResponse>[] inProperties)
        {
            properties = new Dictionary<Function, FunctionalProperty<TSharedValueSet, TSharedAppServiceResponse>>();
            foreach (var property in inProperties)
            {
                if (property == null)
                {
                    Logger.Warn("Null property found, skip it...");
                    continue;
                }

                if (!properties.ContainsKey(property.Function))
                {
                    properties.Add(property.Function, property);
                }
                else
                {
                    Logger.Warn($"Duplicated property {property.Function}");
                }
            }
        }

        public async Task OnRequestReceived(TSharedAppServiceRequest request)
        {
            var function = (Function)request.Message[nameof(Function)];
            if (function == Function.None)
            {
                Logger.Error("Invalid function.");
                return;
            }

            if (!properties.TryGetValue(function, out var property))
            {
                Logger.Error($"Property {function} not found.");
                return;
            }

            var command = (Command)request.Message[nameof(Command)];
            var response = new TSharedValueSet();
            switch (command)
            {
                case Command.Get:
                    response = property.AddValueSetContent(response);
                    break;
                case Command.Set:
                    property.SetValue(request.Message[nameof(Content)], (long)request.Message[nameof(UpdatedTime)]);
                    response.Add(nameof(Content), "Set Success");
                    break;
                default:
                    Logger.Error($"Can't process command {command}");
                    break;
            }
            Logger.Info($"Start sending response {function} {response.ToDebugString()}");
            var sendResponseResult = await SendResponse(request, response);
            Logger.Info($"Sent response {function} {sendResponseResult}.");
        }

        protected abstract Task<SharedAppServiceResponseStatus> SendResponse(TSharedAppServiceRequest request, TSharedValueSet response);
    }
}
