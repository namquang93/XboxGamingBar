using Shared.Enums;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Power
{
    internal class AutoTDPEnabledProperty : HelperProperty<bool, AutoTDPManager>
    {
        public AutoTDPEnabledProperty(bool inValue, AutoTDPManager inManager) : base(inValue, null, Function.AutoTDPEnabled, inManager)
        {
        }
    }
}
