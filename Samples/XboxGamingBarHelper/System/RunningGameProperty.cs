using Shared.Data;
using Shared.Enums;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.System
{
    internal class RunningGameProperty : AppServiceConnectionProperty<RunningGame>
    {
        public RunningGameProperty(RunningGame initialValue, AppServiceConnection initialConnection, Function initialFunction) : base(initialValue, null, initialConnection, initialFunction)
        {
        }

        protected override ValueSet AddContent(ValueSet inValueSet)
        {
            inValueSet.Add(nameof(Content), Value.ToString());
            return inValueSet;
        }
    }
}
