using Shared.Data;
using Shared.Enums;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Power
{
    internal class TargetFPSProperty : HelperProperty<int, AutoTDPController>
    {
        public TargetFPSProperty(int inValue, IProperty inParentProperty, AutoTDPController inManager) : base(inValue, inParentProperty, Function.TargetFPS, inManager)
        {
        }

        protected override void NotifyPropertyChanged(string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);
            Manager.TargetFPS = Value;
        }
    }
}
