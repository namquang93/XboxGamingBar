using Shared.Constants;
using Shared.Enums;
using System.Runtime.CompilerServices;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Power
{
    internal class CPUClockMaxProperty : HelperProperty<int, PowerManager>
    {
        public CPUClockMaxProperty(PowerManager inManager) : base(CPUConstants.DEFAULT_CPU_CLOCK, null, Function.CPUClockMax, inManager)
        {
        }

        protected override void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            PowerManager.SetCpuFreqLimit(true, (uint)Value, false);
            PowerManager.SetCpuFreqLimit(false, (uint)Value, false);
            PowerManager.SetCpuFreqLimit(true, (uint)Value, true);
            PowerManager.SetCpuFreqLimit(false, (uint)Value, true);
        }
    }
}
