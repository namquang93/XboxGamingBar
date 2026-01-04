using Shared.Data;
using Windows.ApplicationModel.AppService;

namespace XboxGamingBar.Data
{
    internal class WidgetAppServiceResponse : SharedAppServiceResponse
    {
        public AppServiceResponse AppServiceResponse { get; }

        public WidgetAppServiceResponse(AppServiceResponse appServiceResponse)
        {
            AppServiceResponse = appServiceResponse;
        }
    }
}
