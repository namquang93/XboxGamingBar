using Shared.Data;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;

namespace XboxGamingBarHelper.Core
{
    internal class HelperAppServiceRequest : SharedAppServiceRequest
    {
        public AppServiceRequest Request { get; }

        public override SharedValueSet Message
        {
            get { return new HelperValueSet(Request.Message); }
        }

        public HelperAppServiceRequest(AppServiceRequest request)
        {
            Request = request;
        }

        internal IAsyncOperation<AppServiceResponseStatus> SendResponseAsync(HelperValueSet response)
        {
            return Request.SendResponseAsync(response.ValueSet);
        }
    }
}
