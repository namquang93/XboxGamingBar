using LibreHardwareMonitor.Hardware;
using System.Drawing;

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
            switch (osdLevel)
            {
                case 3: // for level 3, only show CPU usage and temperature.
                    return new OSDItemValue[]
                    {
                        new OSDItemValue(cpuUsageSensor != null ? cpuUsageSensor.Value.Value : -1.0f, "%"),
                        new OSDItemValue(cpuTemperatureSensor != null ? cpuTemperatureSensor.Value.Value : -1.0f, "°C")
                    };
                case 4: // for level 4, show CPU usage, clock speed, wattage and temperature.
                    return new OSDItemValue[]
                    {
                        new OSDItemValue(cpuUsageSensor != null ? cpuUsageSensor.Value.Value : -1.0f, "%"),
                        new OSDItemValue(cpuClockSensor != null ? cpuClockSensor.Value.Value : -1.0f, "MHz"),
                        new OSDItemValue(cpuWattageSensor != null ? cpuWattageSensor.Value.Value : -1.0f, "W"),
                        new OSDItemValue(cpuTemperatureSensor != null ? cpuTemperatureSensor.Value.Value : -1.0f, "°C")
                    };
                default: // otherwise, return nothing.
                    return base.GetValues(osdLevel);
            }
        }
    }
}
