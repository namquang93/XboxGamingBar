using Shared.Enums;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.AMD.Properties
{
    internal class AMDAntiLagSupportedProperty : HelperProperty<bool, AMDManager>
    {
        public AMDAntiLagSupportedProperty(bool inValue, AMDManager inManager) : base(inValue, null, Function.AMDAntiLagSupported, inManager)
        {
        }
    }

    internal class AMDAntiLagEnabledProperty : HelperProperty<bool, AMDManager>
    {
        public AMDAntiLagEnabledProperty(bool inValue, AMDManager inManager) : base(inValue, null, Function.AMDAntiLagEnabled, inManager)
        {
        }

        protected override void NotifyPropertyChanged(string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            Manager.AMDAntiLagSetting.SetEnabled(Value);
        }
    }
}
