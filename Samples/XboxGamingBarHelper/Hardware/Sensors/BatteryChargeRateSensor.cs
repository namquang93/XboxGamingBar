#if !STORE
using LibreHardwareMonitor.Hardware;
#endif

namespace XboxGamingBarHelper.Hardware.Sensors
{
    internal class BatteryChargeRateSensor : HardwareSensor
    {
        public BatteryChargeRateSensor() : base("Charge Rate", HardwareType.Battery, SensorType.Power)
        {
        }
    }
}
