#if !STORE
using LibreHardwareMonitor.Hardware;
using NLog;
using System;
using System.Collections.Generic;
using XboxGamingBarHelper.Core;
using XboxGamingBarHelper.Hardware.Sensors;

namespace XboxGamingBarHelper.Hardware
{
    internal class LibreHardwareProvider : IHardwareProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Computer computer;
        private readonly IVisitor updateVisitor;
        private string cpuName = string.Empty;
        private string motherboardName = string.Empty;

        public LibreHardwareProvider()
        {
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsNetworkEnabled = true,
                IsStorageEnabled = true,
                IsBatteryEnabled = true,
            };
            updateVisitor = new UpdateVisitor();
            computer.Open();

            foreach (IHardware hardware in computer.Hardware)
            {
                var properties = string.Empty;
                if (hardware.Properties.Count > 0)
                {
                    foreach (var property in hardware.Properties)
                    {
                        properties = properties.Length == 0 ? $"{property.Key}:{property.Value}" : $"{properties}, {property.Key}:{property.Value}";
                    }
                }

                Logger.Info($"Found hardware {hardware.HardwareType}: Name={hardware.Name}, Type={hardware.HardwareType}, Id={hardware.Identifier}, Properties={properties}");
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    cpuName = hardware.Name;
                }

                if (hardware.HardwareType == HardwareType.Motherboard)
                {
                    motherboardName = hardware.Name;
                }
            }
        }

        public void Update()
        {
            computer.Accept(updateVisitor);
        }

        private float GetSensorValue(HardwareType hardwareType, SensorType sensorType, string name)
        {
            foreach (var hardware in computer.Hardware)
            {
                if (hardware.HardwareType != hardwareType) continue;

                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == sensorType && sensor.Name == name)
                    {
                        return sensor.Value ?? -1.0f;
                    }
                }
            }
            return -1.0f;
        }

        public float GetCpuClock() => GetSensorValue(HardwareType.Cpu, SensorType.Clock, "Core #1");
        public float GetCpuUsage() => GetSensorValue(HardwareType.Cpu, SensorType.Load, "CPU Total");
        public float GetCpuWattage() => GetSensorValue(HardwareType.Cpu, SensorType.Power, "Package");
        public float GetCpuTemperature() => GetSensorValue(HardwareType.Cpu, SensorType.Temperature, "Core (Tctl/Tdie)");

        public float GetGpuClock() => GetSensorValue(HardwareType.GpuAmd, SensorType.Clock, "GPU Core");
        public float GetGpuUsage() => GetSensorValue(HardwareType.GpuAmd, SensorType.Load, "GPU Core");
        public float GetGpuWattage() => GetSensorValue(HardwareType.GpuAmd, SensorType.Power, "GPU Core");
        public float GetGpuTemperature() => GetSensorValue(HardwareType.GpuAmd, SensorType.Temperature, "GPU VR SoC");

        public float GetMemoryUsage() => GetSensorValue(HardwareType.Memory, SensorType.Load, "Memory");
        public float GetMemoryUsed() => GetSensorValue(HardwareType.Memory, SensorType.Data, "Memory Used");

        public float GetBatteryLevel() => GetSensorValue(HardwareType.Battery, SensorType.Level, "Charge Level");
        public float GetBatteryRemainingTime() => GetSensorValue(HardwareType.Battery, SensorType.TimeSpan, "Remaining Time (Estimated)");
        public float GetBatteryDischargeRate() => GetSensorValue(HardwareType.Battery, SensorType.Power, "Discharge Rate");
        public float GetBatteryChargeRate() => GetSensorValue(HardwareType.Battery, SensorType.Power, "Charge Rate");

        public string GetCpuName() => cpuName;
        public string GetMotherboardName() => motherboardName;
    }
}
#endif
