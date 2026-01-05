using Shared.Data;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;

namespace XboxGamingBar.Data
{
    internal class WidgetAppServiceRequest : SharedAppServiceRequest
    {
        public AppServiceRequest Request { get; }

        public override SharedValueSet Message
        {
            get { return new WidgetValueSet(Request.Message); }
        }

        public WidgetAppServiceRequest(AppServiceRequest request)
        {
            Request = request;
        }

        internal IAsyncOperation<AppServiceResponseStatus> SendResponseAsync(WidgetValueSet response)
        {
            return Request.SendResponseAsync(response.ValueSet);
        }
    }
}
