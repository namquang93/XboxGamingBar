#if !STORE
using LibreHardwareMonitor.Hardware;
#endif

namespace XboxGamingBarHelper.Hardware.Sensors
{
    internal class MemoryUsageSensor : HardwareSensor
    {
        public MemoryUsageSensor() : base("Memory", HardwareType.Memory, SensorType.Load)
        {
        }
    }
}
