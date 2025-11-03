namespace XboxGamingBarHelper.AMD
{
    internal abstract class AMDSetting<SettingType> where SettingType : IADLXInterface
    {
        public abstract bool IsSupported();
        public abstract bool IsEnabled();
        public abstract void SetEnabled(bool enabled);

        protected SettingType adlxSetting;
        public SettingType ADLXSetting { get {  return adlxSetting; } }

        public AMDSetting(SettingType setting)
        {
            adlxSetting = setting;
        }

        ~AMDSetting()
        {
            adlxSetting?.Release();
        }

        public virtual int Release()
        {
            if (adlxSetting == null) return 0;

            return adlxSetting.Release();
        }
    }
}
