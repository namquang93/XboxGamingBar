using NLog;
using Shared.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace Shared.Data
{
    public abstract class FunctionalProperties
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected readonly Dictionary<Function, FunctionalProperty> properties;

        public FunctionalProperties(params FunctionalProperty[] inProperties)
        {
            properties = new Dictionary<Function, FunctionalProperty>();
            foreach (var property in inProperties)
            {
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

        public async Task OnRequestReceived(AppServiceRequest request)
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
            var response = new ValueSet();
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

        protected abstract Task<AppServiceResponseStatus> SendResponse(AppServiceRequest request, ValueSet response);
    }
}
