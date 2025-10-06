using Shared.Data;
using Shared.Enums;
using Windows.ApplicationModel.AppService;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.RTSS
{
    internal class OSDProperty : AppServiceConnectionProperty<int>
    {
        public OSDProperty(int initialValue, IProperty initialParentProperty, AppServiceConnection initialConnection, Function initialFunction) : base(initialValue, initialParentProperty, initialConnection, initialFunction)
        {
        }
    }
}
