using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace XboxGamingBarHelper.RTSS
{
    internal abstract class OSDItem
    {
        private const string SingleLineSpacing = "  ";
        private const string MultipleLinesSpacing = "\t";
        private const string MultipleLinesSpacing2 = "\t\t";

        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected string name;
        protected string colorCode;

        protected OSDItem()
        {
            name = "OSD Item";
            colorCode = "FFFFFF";
        }

        protected OSDItem(string name, Color color)
        {
            this.name = name;
            this.colorCode = $"{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public virtual string GetOSDString(int osdLevel)
        {
            var osdValues = GetValues(osdLevel);

            if (osdValues == null || osdValues.Count == 0)
            {
                return string.Empty;
            }

            var osdString = $"{GetNameString()}<C=FFFFFF>{(osdLevel >= 3 ? MultipleLinesSpacing2 : SingleLineSpacing)}";

            //if (osdValues == null || osdValues.Count == 0)
            //{
            //    return osdString + " N/A";
            //}

            var lineBreakCounter = 0;
            for (int i = 0; i < osdValues.Count; i++)
            {
                var osdValue = osdValues[i];
                if (osdValue.Value < 0)
                {
                    //osdString += "N/A";
                }
                else
                {
                    if (osdValue.ShouldFloorToInt)
                    {
                        osdString += $"{osdValue.Prefix}{Math.Floor(osdValue.Value)}<S=50> {osdValue.Unit}<S>";
                    }
                    else
                    {
                        osdString += $"{osdValue.Prefix}{osdValue.Value:F1}<S=50> {osdValue.Unit}<S>";
                    }
                }

                if (i < osdValues.Count - 1)
                {
                    lineBreakCounter++;
                    if (osdLevel >= 3 && lineBreakCounter >= 2)
                    {
                        lineBreakCounter = 0;
                        osdString += "\n\t";
                    }

                    osdString += osdLevel >= 3 ? MultipleLinesSpacing : SingleLineSpacing;
                }
            }
            osdString += "<C>";

            return osdString;
        }

        protected virtual string GetNameString()
        {
            return $"<C={colorCode}>{name}<C>";
        }

        protected virtual List<OSDItemValue> GetValues(int osdLevel)
        {
            return new List<OSDItemValue>();
        }
    }
}
