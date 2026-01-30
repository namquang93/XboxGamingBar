using Shared.Data;
using Shared.Enums;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Power
{
    internal class AutoTDPEnabledProperty : HelperProperty<bool, AutoTDPController>
    {
        public AutoTDPEnabledProperty(bool inValue, IProperty inParentProperty, AutoTDPController inManager) : base(inValue, inParentProperty, Function.AutoTDPEnabled, inManager)
        {
        }

        protected override void NotifyPropertyChanged(string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);
            Manager.IsEnabled = Value;
        }
    }
}
