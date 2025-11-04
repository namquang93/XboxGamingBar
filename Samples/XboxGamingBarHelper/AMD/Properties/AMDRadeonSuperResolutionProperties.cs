using Shared.Enums;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.AMD.Properties
{
    internal class AMDRadeonSuperResolutionSupportedProperty : HelperProperty<bool, AMDManager>
    {
        public AMDRadeonSuperResolutionSupportedProperty(bool inValue, AMDManager inManager) : base(inValue, null, Function.AMDRadeonSuperResolutionSupported, inManager)
        {
        }
    }

    internal class AMDRadeonSuperResolutionEnabledProperty : HelperProperty<bool, AMDManager>
    {
        public AMDRadeonSuperResolutionEnabledProperty(bool inValue, AMDManager inManager) : base(inValue, null, Function.AMDRadeonSuperResolutionEnabled, inManager)
        {
        }
    }
}
