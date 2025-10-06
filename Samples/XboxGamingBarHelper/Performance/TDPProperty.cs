using Shared.Data;
using Shared.Enums;
using Windows.ApplicationModel.AppService;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Performance
{
    internal class TDPProperty : AppServiceConnectionProperty<int>
    {
        public TDPProperty(int initialValue, IProperty initialParentProperty, AppServiceConnection initialConnection, Function initialFunction) : base(initialValue, initialParentProperty, initialConnection, initialFunction)
        {
        }
    }
}
