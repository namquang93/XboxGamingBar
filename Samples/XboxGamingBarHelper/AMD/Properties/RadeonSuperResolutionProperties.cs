using Shared.Enums;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.AMD.Properties
{
    internal class RadeonSuperResolutionSupportedProperty : HelperProperty<bool, AMDManager>
    {
        public RadeonSuperResolutionSupportedProperty(bool inValue, AMDManager inManager) : base(inValue, null, Function.RadeonSuperResolutionSupported, inManager)
        {
        }
    }

    internal class RadeonSuperResolutionEnabledProperty : HelperProperty<bool, AMDManager>
    {
        public RadeonSuperResolutionEnabledProperty(bool inValue, AMDManager inManager) : base(inValue, null, Function.RadeonSuperResolutionEnabled, inManager)
        {
        }
    }
}
