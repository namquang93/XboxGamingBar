using Shared.Data;
using Shared.Enums;
using System.Collections.Generic;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Settings
{
    internal class LosslessScalingShortcutProperty : HelperProperty<List<int>, SettingsManager>
    {
        public LosslessScalingShortcutProperty(List<int> inValue, IProperty inParentProperty, SettingsManager inManager) : base(inValue, inParentProperty, Function.LosslessScalingShortcut, inManager)
        {
        }

        protected override void NotifyPropertyChanged(string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);
            
            // This will trigger the save logic in SettingsManager if we hook it up.
        }
    }
}
