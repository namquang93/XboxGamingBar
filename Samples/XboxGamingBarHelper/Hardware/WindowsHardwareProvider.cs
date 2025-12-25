using NLog;
using System.Windows.Forms;
using XboxGamingBarHelper.Core;
using XboxGamingBarHelper.Power;

namespace XboxGamingBarHelper.Hardware
{
    internal class WindowsHardwareProvider : IHardwareProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private float batteryLevel = -1.0f;
        private float batteryRemainingTime = -1.0f;
        private float batteryDischargeRate = -1.0f;
        private float batteryChargeRate = -1.0f;

        public void Update()
        {
            var powerStatus = System.Windows.Forms.SystemInformation.PowerStatus;

            batteryLevel = powerStatus.BatteryLifePercent * 100;
            batteryRemainingTime = powerStatus.BatteryLifeRemaining;

            if (PowerManager.TryGetBatteryState(out var battery))
            {
                if (battery.Charging)
                {
                    batteryDischargeRate = -1.0f;
                    batteryChargeRate = battery.Rate / 1000.0f;
                }
                else
                {
                    batteryDischargeRate = battery.Rate / 1000.0f;
                    batteryChargeRate = -1.0f;
                }
            }
            else
            {
                Logger.Warn("Can't get battery charge/discharge rate.");
            }
        }

        public float GetCpuClock() => -1.0f;
        public float GetCpuUsage() => -1.0f;
        public float GetCpuWattage() => -1.0f;
        public float GetCpuTemperature() => -1.0f;

        public float GetGpuClock() => -1.0f;
        public float GetGpuUsage() => -1.0f;
        public float GetGpuWattage() => -1.0f;
        public float GetGpuTemperature() => -1.0f;

        public float GetMemoryUsage() => -1.0f;
        public float GetMemoryUsed() => -1.0f;

        public float GetBatteryLevel() => batteryLevel;
        public float GetBatteryRemainingTime() => batteryRemainingTime;
        public float GetBatteryDischargeRate() => batteryDischargeRate;
        public float GetBatteryChargeRate() => batteryChargeRate;

        public string GetCpuName() => string.Empty;
        public string GetMotherboardName() => string.Empty;
    }
}
