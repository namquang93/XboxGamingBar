using NLog;
using System;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.AppService;
using XboxGamingBarHelper.Core;
using XboxGamingBarHelper.Windows;

namespace XboxGamingBarHelper.Power
{
    internal class PowerManager : Manager
    {
        private readonly CPUBoostProperty cpuBoost;
        public CPUBoostProperty CPUBoost
        {
            get { return cpuBoost; }
        }

        private readonly CPUEPPProperty cpuEPP;
        public CPUEPPProperty CPUEPP
        {
            get { return cpuEPP; }
        }

        public PowerManager(AppServiceConnection connection) : base(connection)
        {
            cpuBoost = new CPUBoostProperty(GetCpuBoostMode(/*false*/), this);
            cpuEPP = new CPUEPPProperty((int)GetEppValue(), this);
        }

        public static Guid GetActiveScheme()
        {
            var res = PowerProf.PowerGetActiveScheme(IntPtr.Zero, out IntPtr pGuid);
            if (res != 0)
            {
                Logger.Error("Can't get active power scheme?");
                return Guid.Empty;
            }

            var active = (Guid)Marshal.PtrToStructure(pGuid, typeof(Guid));
            Marshal.FreeHGlobal(pGuid);
            return active;
        }

        public static bool GetCpuBoostMode(/*bool isAC*/)
        {
            var isAC = false;
            var scheme = GetActiveScheme();
            var subgroup = PowerGuids.GUID_PROCESSOR_SETTINGS_SUBGROUP;
            var setting = PowerGuids.GUID_PROCESSOR_PERFBOOST_MODE;

            var status = isAC
                ? PowerProf.PowerReadACValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, out uint result)
                : PowerProf.PowerReadDCValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, out result);

            if (status != 0)
            {
                Logger.Error("Can't get CPU Boost Mode?");
                return false;
            }

            return result != 0;
        }

        public static void SetCpuBoostMode(/*bool isAC, */bool enabled)
        {
            var scheme = GetActiveScheme();
            var subgroup = PowerGuids.GUID_PROCESSOR_SETTINGS_SUBGROUP;
            var setting = PowerGuids.GUID_PROCESSOR_PERFBOOST_MODE;
            uint value = (uint)(enabled ? 2 : 0);
            Logger.Info($"Set CPU Boost to {(enabled ? "Aggressive" : "Disabled")}.");

            //var status = isAC ? 
            //    PowerProf.PowerWriteACValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, value)
            //    :
            //    PowerProf.PowerWriteDCValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, value);
            var status = PowerProf.PowerWriteACValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, value);
            if (status == 0)
            {
                status = PowerProf.PowerWriteDCValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, value);
            }

            if (status != 0)
            {
                Logger.Error("Can't set CPU Boost Mode??");
                return;
            }

            // Apply the updated plan
            PowerProf.PowerSetActiveScheme(IntPtr.Zero, ref scheme);
        }

        public static uint GetEppValue(/*bool isAC*/)
        {
            Guid scheme = GetActiveScheme();
            Guid subgroup = PowerGuids.GUID_PROCESSOR_SETTINGS_SUBGROUP;
            Guid setting = PowerGuids.GUID_PROCESSOR_EPP;
            var isAC = false;

            uint result;
            uint status = isAC ? 
                PowerProf.PowerReadACValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, out result)
                : PowerProf.PowerReadDCValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, out result);

            if (status != 0)
            {
                Logger.Error("Can't get EPP value.");
                return 90;
            }

            return result;
        }

        public static void SetEppValue(/*bool isAC, */uint value)
        {
            if (value > 100) value = 100; // clamp to valid range

            Guid scheme = GetActiveScheme();
            Guid subgroup = PowerGuids.GUID_PROCESSOR_SETTINGS_SUBGROUP;
            Guid setting = PowerGuids.GUID_PROCESSOR_EPP;

            //uint status = isAC
            //    ? PowerProf.PowerWriteACValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, value)
            //    : PowerProf.PowerWriteDCValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, value);

            var status = PowerProf.PowerWriteACValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, value);
            if (status == 0)
            {
                status = PowerProf.PowerWriteDCValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, value);
            }

            if (status != 0)
            {
                Logger.Error("Can't set EPP value.");
            }

            // Apply changes to the currently active power plan
            PowerProf.PowerSetActiveScheme(IntPtr.Zero, ref scheme);
        }
    }
}
