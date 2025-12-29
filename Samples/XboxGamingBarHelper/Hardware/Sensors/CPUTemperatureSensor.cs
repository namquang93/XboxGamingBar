#if !STORE
using LibreHardwareMonitor.Hardware;
#endif

namespace XboxGamingBarHelper.Hardware.Sensors
{
    internal class CPUTemperatureSensor : HardwareSensor
    {
        public CPUTemperatureSensor() : base("Core (Tctl/Tdie)", HardwareType.Cpu, SensorType.Temperature)
        {
        }
    }
}
