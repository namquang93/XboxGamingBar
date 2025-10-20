using Shared.Enums;
using System.Runtime.CompilerServices;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Power
{
    internal class CPUClockLimitProperty : HelperProperty<bool, PowerManager>
    {
        public CPUClockLimitProperty(bool inValue, PowerManager inManager) : base(inValue, null, Function.CPUClockLimit, inManager)
        {
        }

        protected override void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            Logger.Info($"{(Value ? "Enable" : "Disable")} CPU Clock limit.");
            var cpuClockLimit = (uint)(Value ? Manager.CPUClockMax : 0);
            PowerManager.SetCpuFreqLimit(true, cpuClockLimit, false);
            PowerManager.SetCpuFreqLimit(false, cpuClockLimit, false);
            PowerManager.SetCpuFreqLimit(true, cpuClockLimit, true);
            PowerManager.SetCpuFreqLimit(false, cpuClockLimit, true);
        }
    }
}
