using Shared.Data;
using Shared.Enums;
using Shared.Utilities;
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

        public override bool SetValue(object value)
        {
            if (value is string stringValue)
            {
                return base.SetValue(XmlHelper.FromXMLString<RunningGame>(stringValue));
            }
            else
            {
                return base.SetValue(value);
            }
        }

        public override ValueSet AddValueSetContent(in ValueSet inValueSet)
        {
            inValueSet.Add(nameof(Content), XmlHelper.ToXMLString(Value, true));
            return inValueSet;
        }

        protected override void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            // Manager.RunningGame = Value;
        }
    }
}
