using Shared.Enums;
using System.Collections.Generic;
using System.Text;

namespace Shared.Utilities
{
    public static class DebugHelper
    {
        private const string QUOTE = "'";
        private const string SPACE = " ";
        public static string ToDebugString(this IDictionary<string, object> valueSet)
        {
            if (valueSet == null || valueSet.Count == 0)
            {
                return string.Empty;
            }

            var debugString = new StringBuilder();
            debugString.Append(QUOTE);

            if (valueSet.ContainsKey(nameof(Command)))
            {
                debugString.Append((Command)valueSet[nameof(Command)]);
            }

            var hasFunction = false;
            if (valueSet.ContainsKey(nameof(Function)))
            {
                hasFunction = true;
                debugString.Append(SPACE);
                debugString.Append((Function)valueSet[nameof(Function)]);
            }

            if (valueSet.ContainsKey(nameof(Content)))
            {
                if (hasFunction)
                {
                    debugString.Append(" to ");
                }
                debugString.Append(valueSet[nameof(Content)]);
            }

            if (valueSet.ContainsKey(nameof(UpdatedTime)))
            {
                debugString.Append(" at ");
                debugString.Append(valueSet[nameof(UpdatedTime)]);
            }

            debugString.Append(QUOTE);
            return debugString.ToString();
        }
    }
}
