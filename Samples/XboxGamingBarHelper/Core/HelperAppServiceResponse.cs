using Shared.Data;
using Windows.ApplicationModel.AppService;

namespace XboxGamingBarHelper.Core
{
    internal class HelperAppServiceResponse : SharedAppServiceResponse
    {
        public AppServiceResponse AppServiceResponse { get; }

        public HelperAppServiceResponse(AppServiceResponse appServiceResponse)
        {
            AppServiceResponse = appServiceResponse;
        }
    }
}
