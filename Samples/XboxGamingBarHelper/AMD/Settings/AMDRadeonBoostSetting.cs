namespace XboxGamingBarHelper.AMD.Settings
{
    internal class AMDRadeonBoostSetting : AMDSetting<IADLX3DBoost>
    {
        public AMDRadeonBoostSetting(IADLX3DBoost setting) : base(setting)
        {
        }
        public override bool IsSupported()
        {
            return AMDUtilities.GetBoolValue(adlxSetting.IsSupported);
        }
        public override bool IsEnabled()
        {
            return AMDUtilities.GetBoolValue(adlxSetting.IsEnabled);
        }
        public override void SetEnabled(bool enabled)
        {
            adlxSetting.SetEnabled(enabled);
        }
    }
}
