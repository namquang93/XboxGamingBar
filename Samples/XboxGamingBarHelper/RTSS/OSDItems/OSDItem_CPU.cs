using LibreHardwareMonitor.Hardware;
using System.Diagnostics;
using System.Drawing;
using XboxGamingBarHelper.Performance.Sensors;

namespace XboxGamingBarHelper.RTSS.OSDItems
{
    internal class OSDItem_CPU : OSDItem
    {
        private ISensor cpuUsageSensor;
        private ISensor cpuClockSensor;
        private ISensor cpuWattageSensor;
        private ISensor cpuTemperatureSensor;

        public OSDItem_CPU(ISensor cpuUsageSensor, ISensor cpuClockSensor, ISensor cpuWattageSensor, ISensor cpuTemperatureSensor) : base("CPU", Color.FromArgb(0x09, 0x79, 0x69))
        {
            this.cpuWattageSensor = cpuWattageSensor;
            this.cpuUsageSensor = cpuUsageSensor;
            this.cpuClockSensor = cpuClockSensor;
            this.cpuTemperatureSensor = cpuTemperatureSensor;
        }

        protected override OSDItemValue[] GetValues(int osdLevel)
        {
            if (cpuWattageSensor == null || cpuUsageSensor == null || cpuClockSensor == null || cpuTemperatureSensor == null)
            {
                Debug.WriteLine($"OSDItem_CPU: Invalid config, CPU Wattage Sensor {(cpuWattageSensor == null ? "valid" : "invalid")}, CPU Usage Sensor {(cpuUsageSensor == null ? "valid" : "invalid")}, CPU Clock Sensor {(cpuClockSensor == null ? "valid" : "invalid")}, CPU Temperature Sensor {(cpuTemperatureSensor == null ? "valid" : "invalid")}");
                return base.GetValues(osdLevel);
            }
            float f = 13.5f;
            string s = f.ToString("R");

            switch (osdLevel)
            {
                case 3: // for level 3, only show CPU usage and temperature.
                    return new OSDItemValue[]
                    {
                        new OSDItemValue(cpuUsageSensor.Value?.ToString("0.0") ?? "N/A", "%"),
                        new OSDItemValue(cpuTemperatureSensor.Value?.ToString("0.0") ?? "N/A", "°C")
                    };
                case 4: // for level 4, show CPU usage, clock speed, wattage and temperature.
                    return new OSDItemValue[]
                    {
                        new OSDItemValue(cpuUsageSensor.Value?.ToString("0.0") ?? "N/A", "%"),
                        new OSDItemValue(cpuClockSensor.Value?.ToString("0.0") ?? "N/A", "MHz"),
                        new OSDItemValue(cpuWattageSensor.Value?.ToString("0.0") ?? "N/A", "W"),
                        new OSDItemValue(cpuTemperatureSensor.Value?.ToString("0.0") ?? "N/A", "°C")
                    };
                default: // otherwise, return nothing.
                    return base.GetValues(osdLevel);
            }
        }
    }
}
