namespace XboxGamingBarHelper.RTSS
{
    internal readonly struct OSDItemValue
    {
        public float Value { get; }
        public string Unit { get; }
        public string Prefix { get; }
        public bool ShouldFloorToInt { get; }

        public OSDItemValue(float value, string unit)
        {
            Value = value;
            Unit = unit;
            Prefix = string.Empty;
            ShouldFloorToInt = true;
        }

        public OSDItemValue(float value, string unit, string prefix)
        {
            Value = value;
            Unit = unit;
            Prefix = prefix;
            ShouldFloorToInt = true;
        }

        public OSDItemValue(float value, string unit, string prefix, bool shouldFloorToInt)
        {
            Value = value;
            Unit = unit;
            Prefix = prefix;
            ShouldFloorToInt = shouldFloorToInt;
        }
    }
}
