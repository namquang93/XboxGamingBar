namespace XboxGamingBarHelper.Hardware
{
#if STORE
    enum HardwareType
    {
        Cpu,
        Gpu,
        GpuAmd,
        Memory,
        Disk,
        Network,
        Battery
    }

    enum SensorType
    {
        Temperature,
        Load,
        Clock,
        Voltage,
        FanSpeed,
        Power,
        DataRate,
        Usage,
        Level,
        Data,
        TimeSpan
    }
#else
    using LibreHardwareMonitor.Hardware;
#endif

    internal abstract class HardwareSensor
    {
        protected string sensorName;
        public string SensorName
        {
            get { return sensorName; }
        }

        protected HardwareType hardwareType;
        public HardwareType HardwareType
        {
            get { return hardwareType; }
        }

        protected SensorType sensorType;
        public SensorType SensorType
        {
            get { return sensorType; }
        }

        public float Value { get; set; }

        protected HardwareSensor(string inSensorName, HardwareType inHardwareType, SensorType inSensorType)
        {
            sensorName = inSensorName;
            hardwareType = inHardwareType;
            sensorType = inSensorType;
        }
    }
}
