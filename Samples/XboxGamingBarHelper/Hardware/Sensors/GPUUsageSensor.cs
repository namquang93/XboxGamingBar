#if !STORE
using LibreHardwareMonitor.Hardware;
#endif

namespace XboxGamingBarHelper.Hardware.Sensors
{
    internal class GPUUsageSensor : HardwareSensor
    {
        public GPUUsageSensor() : base("GPU Core", HardwareType.GpuAmd, SensorType.Load)
        {
        }
    }
}
