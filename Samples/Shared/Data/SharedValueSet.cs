using System.Collections;

namespace Shared.Data
{
    public class SharedValueSet : IEnumerable
    {
        public virtual object this[string key]
        {
            get { return null; }
        }

        public virtual void Add(string key, object value)
        {

        }

        public virtual bool TryGetValue(string key, out object value)
        {
            value = null;
            return false;
        }

        public virtual string ToDebugString()
        {
            return "{}";
        }

        public virtual IEnumerator GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}
