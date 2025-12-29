using System.Collections.Generic;

namespace XboxGamingBarHelper.Hardware
{
    internal interface IHardwareProvider
    {
        void Update();
        float GetCpuClock();
        float GetCpuUsage();
        float GetCpuWattage();
        float GetCpuTemperature();
        float GetGpuClock();
        float GetGpuUsage();
        float GetGpuWattage();
        float GetGpuTemperature();
        float GetMemoryUsage();
        float GetMemoryUsed();
        float GetBatteryLevel();
        float GetBatteryRemainingTime();
        float GetBatteryDischargeRate();
        float GetBatteryChargeRate();
        string GetCpuName();
        string GetMotherboardName();
    }
}
