using Shared.Enums;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Power
{
    internal class TargetFPSProperty : HelperProperty<int, AutoTDPManager>
    {
        public TargetFPSProperty(int inValue, AutoTDPManager inManager) : base(inValue, null, Function.TargetFPS, inManager)
        {
        }
    }
}
