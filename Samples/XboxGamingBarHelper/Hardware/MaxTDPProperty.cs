using Shared.Enums;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Hardware
{
    internal class MaxTDPProperty : HelperProperty<int, HardwareManager>
    {
        public MaxTDPProperty(int inValue, HardwareManager inManager) : base(inValue, null, Function.MaxTDP, inManager)
        {
        }
    }
}
