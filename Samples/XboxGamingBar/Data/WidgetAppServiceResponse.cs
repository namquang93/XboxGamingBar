using Shared.Data;
using Windows.ApplicationModel.AppService;

namespace XboxGamingBar.Data
{
    internal class WidgetAppServiceResponse : SharedAppServiceResponse
    {
        public AppServiceResponse AppServiceResponse { get; }

        public override SharedValueSet Message
        {
            get { return new WidgetValueSet(AppServiceResponse.Message); }
        }

        public WidgetAppServiceResponse(AppServiceResponse appServiceResponse)
        {
            AppServiceResponse = appServiceResponse;
        }
    }
}
