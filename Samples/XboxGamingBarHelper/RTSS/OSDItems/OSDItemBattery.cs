using Shared.Constants;
using System;
using System.Collections.Generic;
using System.Drawing;
using XboxGamingBarHelper.Performance;

namespace XboxGamingBarHelper.RTSS.OSDItems
{
    internal class OSDItemBattery : OSDItem
    {
        private HardwareSensor batteryPercentSensor;
        private HardwareSensor batteryRemainTimeSensor;

        public OSDItemBattery(HardwareSensor batteryPercentSensor, HardwareSensor batteryRemainTimeSensor) : base("BATTERY", Color.DarkCyan)
        {
            this.batteryPercentSensor = batteryPercentSensor;
            this.batteryRemainTimeSensor = batteryRemainTimeSensor;
        }

        protected override List<OSDItemValue> GetValues(int osdLevel)
        {
            var osdItems = base.GetValues(osdLevel);

            if (osdLevel >= 2)
            {
                osdItems.Add(new OSDItemValue(batteryPercentSensor.Value, "%"));
                if (batteryRemainTimeSensor != null && batteryRemainTimeSensor.Value > 0)
                {
                    var hours = Math.Floor(batteryRemainTimeSensor.Value / MathConstants.SECONDS_PER_HOUR);
                    var minutes = (batteryRemainTimeSensor.Value - hours * MathConstants.SECONDS_PER_HOUR) / MathConstants.SECONDS_PER_MINUTE;
                    osdItems.Add(new OSDItemValue((float)hours, "H"));
                    osdItems.Add(new OSDItemValue((float)minutes, "M"));
                }
            }

            return osdItems;
        }
    }
}
