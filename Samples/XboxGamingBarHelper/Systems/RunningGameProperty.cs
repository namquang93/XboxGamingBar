using Shared.Data;
using Shared.Enums;
using System.Runtime.CompilerServices;
using Windows.Foundation.Collections;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Systems
{
    internal class RunningGameProperty : HelperProperty<RunningGame, SystemManager>
    {
        public RunningGameProperty(RunningGame inValue, SystemManager inManager) : base(inValue, null, Function.CurrentGame, inManager)
        {
        }

        protected override ValueSet AddContent(ValueSet inValueSet)
        {
            inValueSet.Add(nameof(Content), Value.ToString());
            return inValueSet;
        }

        protected override void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            // Manager.RunningGame = Value;
        }
    }
}
