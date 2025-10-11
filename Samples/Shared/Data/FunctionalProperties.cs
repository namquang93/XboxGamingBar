using NLog;
using Shared.Enums;
using System;
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
            if (!properties.TryGetValue(function, out var property))
            {
                Logger.Error($"Property {function} not found.");
                return;
            }

            var command = (Command)request.Message[nameof(Command)];
            switch (command)
            {
                case Command.Get:
                    ValueSet response = new ValueSet();
                    response = property.AddValueSetContent(response);
                    var sendResponseResult = await SendResponse(request, response);
                    Logger.Info($"Sent response {function} {sendResponseResult}");
                    
                    break;
                case Command.Set:
                    property.SetValue(request.Message[nameof(Content)]);
                    break;
                default:
                    Logger.Error($"Can't process command {command}");
                    break;
            }
        }

        protected abstract Task<AppServiceResponseStatus> SendResponse(AppServiceRequest request, ValueSet response);
    }
}
