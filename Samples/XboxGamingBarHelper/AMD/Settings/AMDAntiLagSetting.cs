namespace XboxGamingBarHelper.AMD.Settings
{
    internal class AMDAntiLagSetting : AMDSetting<IADLX3DAntiLag>
    {
        public AMDAntiLagSetting(IADLX3DAntiLag setting) : base(setting)
        {
        }

        public override bool IsEnabled()
        {
            return AMDUtilities.GetBoolValue(adlxSetting.IsEnabled);
        }

        public override bool IsSupported()
        {
            return AMDUtilities.GetBoolValue(adlxSetting.IsSupported);
        }

        public override void SetEnabled(bool enabled)
        {
            adlxSetting.SetEnabled(enabled);
        }
    }
}
