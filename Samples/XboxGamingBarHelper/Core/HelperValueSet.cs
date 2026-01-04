using Shared.Data;
using System.Collections;
using System.Linq;
using Windows.Foundation.Collections;

namespace XboxGamingBarHelper.Core
{
    internal class HelperValueSet : SharedValueSet
    {
        public ValueSet ValueSet { get; }

        public HelperValueSet()
        {
            ValueSet = new ValueSet();
        }

        public HelperValueSet(ValueSet inValueSet)
        {
            ValueSet = inValueSet;
        }

        public override object this[string key]
        {
            get { return ValueSet[key]; }
        }

        public override void Add(string key, object value)
        {
            ValueSet.Add(key, value);
        }

        public override bool TryGetValue(string key, out object value)
        {
            return ValueSet.TryGetValue(key, out value);
        }

        public override string ToDebugString()
        {
            return "{}";
        }

        public override IEnumerator GetEnumerator()
        {
            return ValueSet.AsEnumerable().GetEnumerator();
        }
    }
}
