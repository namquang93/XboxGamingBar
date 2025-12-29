#if !STORE
using LibreHardwareMonitor.Hardware;
#endif

namespace XboxGamingBarHelper.Hardware.Sensors
{
    internal class BatteryDischargeRateSensor : HardwareSensor
    {
        public BatteryDischargeRateSensor() : base("Discharge Rate", HardwareType.Battery, SensorType.Power)
        {
        }
    }
}
