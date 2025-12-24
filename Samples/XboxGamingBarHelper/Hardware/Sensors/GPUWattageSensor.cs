#if !STORE
using LibreHardwareMonitor.Hardware;
#endif

namespace XboxGamingBarHelper.Hardware.Sensors
{
    internal class GPUWattageSensor : HardwareSensor
    {
        public GPUWattageSensor() : base("GPU Core", HardwareType.GpuAmd, SensorType.Power)
        {
        }
    }
}
