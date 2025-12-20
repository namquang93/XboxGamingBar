using Shared.Enums;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.AMD
{
    internal class AMDSettingsSupportedProperty : HelperProperty<bool, AMDManager>
    {
        public AMDSettingsSupportedProperty(bool inValue, AMDManager inManager) : base(inValue, null, Function.Support_AMDSettings, inManager)
        {
        }
    }
}
