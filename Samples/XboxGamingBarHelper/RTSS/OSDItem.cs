using System;
using System.Drawing;

namespace XboxGamingBarHelper.RTSS
{
    internal struct OSDItemValue
    {
        string value;
        public string Value => value;

        string unit;
        public string Unit => unit;

        public OSDItemValue(string value, string unit)
        {
            this.value = value;
            this.unit = unit;
        }
    }

    internal abstract class OSDItem
    {
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
            this.colorCode = ColorTranslator.ToHtml(color).Remove(0, 1);
        }

        public virtual string GetOSDString(int osdLevel)
        {
            var osdString = $"{GetNameString()} <C=FFFFFF>";

            var osdValues = GetValues(osdLevel);
            if (osdValues == null || osdValues.Length == 0)
            {
                return osdString + " N/A";
            }

            for (int i = 0; i < osdValues.Length; i++)
            {
                var osdValue = osdValues[i];
                osdString += $"{osdValue.Value}<S=50> {osdValue.Unit}<S>";
                if (i < osdValues.Length - 1)
                {
                    osdString += " ";
                }
            }
            osdString += "<C>";

            return osdString;
        }

        protected virtual string GetNameString()
        {
            return $"<C={colorCode}>{name}<C>";
        }

        protected virtual OSDItemValue[] GetValues(int osdLevel)
        {
            return Array.Empty<OSDItemValue>();
        }
    }
}
