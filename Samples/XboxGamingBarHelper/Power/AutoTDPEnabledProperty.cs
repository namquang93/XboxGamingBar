using Shared.Enums;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Power
{
    internal class AutoTDPEnabledProperty : HelperProperty<bool, AutoTDPController>
    {
        public AutoTDPEnabledProperty(bool inValue, AutoTDPController inManager) : base(inValue, null, Function.AutoTDPEnabled, inManager)
        {
        }
    }
}
