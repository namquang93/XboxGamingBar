using Shared.Data;
using System.Collections;
using System.Linq;
using Windows.Foundation.Collections;

namespace XboxGamingBar.Data
{
    internal class WidgetValueSet : SharedValueSet
    {
        public ValueSet ValueSet { get; }

        public WidgetValueSet()
        {
            ValueSet = new ValueSet();
        }

        public WidgetValueSet(ValueSet inValueSet)
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
            var debugString = "{";
            foreach (var pair in ValueSet)
            {
                debugString += $"{pair.Key}: {pair.Value}, ";
            }
            debugString = debugString.TrimEnd(' ', ',') + "}";
            return debugString;
        }

        public override IEnumerator GetEnumerator()
        {
            return ValueSet.AsEnumerable().GetEnumerator();
        }
    }
}
