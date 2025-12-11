using Shared.Data;
using Shared.Enums;
using XboxGamingBarHelper.Core;
using XboxGamingBarHelper.Windows;

namespace XboxGamingBarHelper.Systems
{
    internal class ResolutionsProperty : HelperProperty<Resolutions, SystemManager>
    {
        public ResolutionsProperty(Resolutions inValue, SystemManager inManager) : base(inValue, null, Function.Resolutions, inManager)
        {
        }
    }
}
