using System.Text;
using Windows.Foundation.Collections;

namespace Shared.Data
{
    public static class ValueSetExtensions
    {
        public static string ToDebugString(this ValueSet valueSet)
        {
            if (valueSet == null || valueSet.Count == 0)
            {
                return string.Empty;
            }

            var debugString = new StringBuilder();
            foreach (var value in valueSet)
            {
                debugString.Append(value.Key);
                debugString.Append(":");
                debugString.Append(value.Value);
                debugString.Append(",");
            }
            debugString.Remove(debugString.Length - 1, 1);
            return debugString.ToString();
        }
    }
}
