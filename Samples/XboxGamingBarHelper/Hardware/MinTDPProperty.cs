using Shared.Enums;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Hardware
{
    internal class MinTDPProperty : HelperProperty<int, HardwareManager>
    {
        public MinTDPProperty(int inValue, HardwareManager inManager) : base(inValue, null, Function.MinTDP, inManager)
        {
        }
    }
}
