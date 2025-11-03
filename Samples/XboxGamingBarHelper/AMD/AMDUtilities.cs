using NLog;
using System;

namespace XboxGamingBarHelper.AMD
{
    internal static class AMDUtilities
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal static bool GetBoolValue(GetBool func)
        {
            var boolPointer = ADLX.new_boolP();
            var getValueResult = func(boolPointer);
            if (getValueResult != ADLX_RESULT.ADLX_OK)
            {
                ADLX.delete_boolP(boolPointer);
                Logger.Error($"Failed to get AMD bool value. ADLX_RESULT: {getValueResult}");
                return false;
            }

            var supported = ADLX.boolP_value(boolPointer);
            ADLX.delete_boolP(boolPointer);
            return supported;
        }

        internal static Tuple<int, int> GetIntRangeValue(GetIntRange func)
        {
            var intRangePointer = ADLX.new_intRangeP();
            var getValueResult = func(intRangePointer);
            if (getValueResult != ADLX_RESULT.ADLX_OK)
            {
                ADLX.delete_intRangeP(intRangePointer);
                Logger.Error($"Failed to get AMD int range value. ADLX_RESULT: {getValueResult}");
                return new Tuple<int, int>(0, 0);
            }

            var intRange = ADLX.intRangeP_value(intRangePointer);
            ADLX.delete_intRangeP(intRangePointer);
            intRangePointer?.Dispose();
            return new Tuple<int, int>(intRange.minValue, intRange.maxValue);
        }
    }
}
