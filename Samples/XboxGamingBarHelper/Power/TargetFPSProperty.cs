using Shared.Enums;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Power
{
    internal class TargetFPSProperty : HelperProperty<int, AutoTDPController>
    {
        public TargetFPSProperty(int inValue, AutoTDPController inManager) : base(inValue, null, Function.TargetFPS, inManager)
        {
        }
    }
}
