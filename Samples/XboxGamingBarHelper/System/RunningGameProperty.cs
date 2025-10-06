using Shared.Data;
using Shared.Enums;
using Windows.Foundation.Collections;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.System
{
    internal class RunningGameProperty : AppServiceConnectionProperty<RunningGame>
    {
        protected override ValueSet AddContent(ValueSet inValueSet)
        {
            inValueSet.Add(nameof(Content), Value.ToString());
            return inValueSet;
        }
    }
}
