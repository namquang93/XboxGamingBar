using Shared.Enums;
using System.Runtime.CompilerServices;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Power
{
    internal class CPUEPPProperty : HelperProperty<int, PowerManager>
    {
        public CPUEPPProperty(int inValue, PowerManager inManager) : base(inValue, null, Function.CPUEPP, inManager)
        {
        }

        protected override void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            PowerManager.SetEppValue((uint)Value);
        }
    }
}
