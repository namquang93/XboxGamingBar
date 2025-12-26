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

        private System.Diagnostics.PerformanceCounter cpuCounter;
        private float cpuUsage = -1.0f;
        private float cpuClock = -1.0f;

        public WindowsHardwareProvider()
        {
            try
            {
                cpuCounter = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total");
                cpuCounter.NextValue(); // First call usually returns 0
            }
            catch (System.Exception ex)
            {
                Logger.Error(ex, "Failed to initialize CPU counter");
            }
        }

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
                // Logger.Warn("Can't get battery charge/discharge rate.");
            }

            // Update CPU Usage
            if (cpuCounter != null)
            {
                try
                {
                    cpuUsage = cpuCounter.NextValue();
                }
                catch { }
            }

            // Update CPU Clock
            cpuClock = GetCurrentCpuFrequency();
        }

        private float GetCurrentCpuFrequency()
        {
            try
            {
                int coreCount = System.Environment.ProcessorCount;
                int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(XboxGamingBarHelper.Windows.PROCESSOR_POWER_INFORMATION)) * coreCount;
                System.IntPtr buffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);

                try
                {
                    uint ret = XboxGamingBarHelper.Windows.PowrProf.CallNtPowerInformation(
                        (int)XboxGamingBarHelper.Windows.POWER_INFORMATION_LEVEL.ProcessorInformation,
                        System.IntPtr.Zero,
                        0,
                        buffer,
                        size);

                    if (ret == 0) // STATUS_SUCCESS
                    {
                        float maxMhz = 0;
                        long stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(XboxGamingBarHelper.Windows.PROCESSOR_POWER_INFORMATION));
                        for (int i = 0; i < coreCount; i++)
                        {
                            System.IntPtr ptr = (System.IntPtr)((long)buffer + (i * stride));
                            var info = (XboxGamingBarHelper.Windows.PROCESSOR_POWER_INFORMATION)System.Runtime.InteropServices.Marshal.PtrToStructure(ptr, typeof(XboxGamingBarHelper.Windows.PROCESSOR_POWER_INFORMATION));
                            if (info.CurrentMhz > maxMhz) maxMhz = info.CurrentMhz;
                        }
                        return maxMhz;
                    }
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(buffer);
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error(ex, "Failed to get CPU frequency");
            }

            return -1.0f;
        }

        public float GetCpuClock() => cpuClock;
        public float GetCpuUsage() => cpuUsage;
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
