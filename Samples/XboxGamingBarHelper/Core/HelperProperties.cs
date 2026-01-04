using Shared.Data;
using System;
using System.Threading.Tasks;

namespace XboxGamingBarHelper.Core
{
    internal class HelperProperties : FunctionalProperties<HelperValueSet, HelperAppServiceResponse, HelperAppServiceRequest>
    {
        public HelperProperties(params FunctionalProperty<HelperValueSet, HelperAppServiceResponse>[] inProperties) : base(inProperties) { }

        protected override Task<SharedAppServiceResponseStatus> SendResponse(HelperAppServiceRequest request, HelperValueSet response)
        {
            // TODO : Implement this method properly.
            //var a = await request.SendResponseAsync(response);
            return null;
        }
    }
}
