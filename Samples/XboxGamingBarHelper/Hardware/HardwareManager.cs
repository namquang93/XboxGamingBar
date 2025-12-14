using NLog;
using System;
//using System.Collections;
using System.Collections.Generic;
using Windows.ApplicationModel.AppService;
using XboxGamingBarHelper.Core;
using XboxGamingBarHelper.Hardware.Devices;
using XboxGamingBarHelper.Hardware.Sensors;

namespace XboxGamingBarHelper.Hardware
{
    //internal class HardwareSensors : IDictionary<string, HardwareSensor>
    //{
    //    HardwareSensor IDictionary<string, HardwareSensor>.this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    //    ICollection<string> IDictionary<string, HardwareSensor>.Keys => throw new NotImplementedException();

    //    ICollection<HardwareSensor> IDictionary<string, HardwareSensor>.Values => throw new NotImplementedException();

    //    int ICollection<KeyValuePair<string, HardwareSensor>>.Count => throw new NotImplementedException();

    //    bool ICollection<KeyValuePair<string, HardwareSensor>>.IsReadOnly => throw new NotImplementedException();

    //    void IDictionary<string, HardwareSensor>.Add(string key, HardwareSensor value)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    void ICollection<KeyValuePair<string, HardwareSensor>>.Add(KeyValuePair<string, HardwareSensor> item)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    void ICollection<KeyValuePair<string, HardwareSensor>>.Clear()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    bool ICollection<KeyValuePair<string, HardwareSensor>>.Contains(KeyValuePair<string, HardwareSensor> item)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    bool IDictionary<string, HardwareSensor>.ContainsKey(string key)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    void ICollection<KeyValuePair<string, HardwareSensor>>.CopyTo(KeyValuePair<string, HardwareSensor>[] array, int arrayIndex)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    IEnumerator<KeyValuePair<string, HardwareSensor>> IEnumerable<KeyValuePair<string, HardwareSensor>>.GetEnumerator()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    bool IDictionary<string, HardwareSensor>.Remove(string key)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    bool ICollection<KeyValuePair<string, HardwareSensor>>.Remove(KeyValuePair<string, HardwareSensor> item)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    bool IDictionary<string, HardwareSensor>.TryGetValue(string key, out HardwareSensor value)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    internal class HardwareManager : Manager
    {
        private readonly CPU cpu;
        private readonly Device device;

        private readonly IntPtr ryzenAdjHandle;

        public CPUUsageSensor CPUUsage { get; }
        public CPUClockSensor CPUClock { get; }
        public CPUWattageSensor CPUWattage { get ; }
        public CPUTemperatureSensor CPUTemperature { get; }

        public GPUUsageSensor GPUUsage { get; }
        public GPUClockSensor GPUClock { get; }
        public GPUWattageSensor GPUWattage { get; }
        public GPUTemperatureSensor GPUTemperature { get; }

        public MemoryUsageSensor MemoryUsage { get; }
        public MemoryUsedSensor MemoryUsed { get; }

        public BatteryLevelSensor BatteryLevel { get; }
        public BatteryRemainingTimeSensor BatteryRemainingTime { get; }
        public BatteryDischargeRateSensor BatteryDischargeRate { get; }
        public BatteryChargeRateSensor BatteryChargeRate { get; }

        private readonly List<HardwareSensor> hardwareSensors;

        private readonly TDPProperty tdp;
        public TDPProperty TDP
        {
            get { return tdp; }
        }

        private readonly MinTDPProperty minTDP;
        public MinTDPProperty MinTDP
        {
            get { return minTDP; }
        }

        private readonly MaxTDPProperty maxTDP;
        public MaxTDPProperty MaxTDP
        {
            get { return maxTDP; }
        }

        internal HardwareManager(AppServiceConnection connection) : base(connection)
        {
            // Initialize the computer sensors
            
            var cpuId = string.Empty;
            var mainboardId = string.Empty;
            
            cpu = CPUFactory.Create(cpuId);
            Logger.Info($"Initialized CPU: {cpu.Name} (\"{cpuId}\")");
            device = DeviceFactory.Create(mainboardId, cpu);
            Logger.Info($"Initialized Device: {device.Name} (\"{mainboardId}\")");

            // Initialize hardware sensors
            CPUClock = new CPUClockSensor();
            CPUUsage = new CPUUsageSensor();
            CPUWattage = new CPUWattageSensor();
            CPUTemperature = new CPUTemperatureSensor();
            GPUUsage = new GPUUsageSensor();
            GPUClock = new GPUClockSensor();
            GPUTemperature = new GPUTemperatureSensor();
            GPUWattage = new GPUWattageSensor();
            MemoryUsage = new MemoryUsageSensor();
            MemoryUsed = new MemoryUsedSensor();
            BatteryLevel = new BatteryLevelSensor();
            BatteryRemainingTime = new BatteryRemainingTimeSensor();
            BatteryDischargeRate = new BatteryDischargeRateSensor();
            BatteryChargeRate = new BatteryChargeRateSensor();
            hardwareSensors = new List<HardwareSensor>()
            {
                CPUClock,
                CPUUsage,
                CPUWattage,
                CPUTemperature,
                GPUUsage,
                GPUClock,
                GPUTemperature,
                GPUWattage,
                MemoryUsage,
                MemoryUsed,
                BatteryLevel,
                BatteryRemainingTime,
                BatteryDischargeRate,
                BatteryChargeRate,
            };

            ryzenAdjHandle = RyzenAdj.init_ryzenadj();
            var initialTDP = 25;
            if (ryzenAdjHandle == IntPtr.Zero)
            {
                Logger.Error("RyzenAdj initialized failed.");
            }
            else
            {
                RyzenAdj.refresh_table(ryzenAdjHandle);
                // RyzenAdj.set_fast_limit(ryzenAdjHandle, 30000);
                initialTDP = (int)RyzenAdj.get_fast_limit(ryzenAdjHandle);
                Logger.Info($"RyzenAdj initialized successfully at {initialTDP}W.");
            }

            minTDP = new MinTDPProperty(device.GetMinTDP(), this);
            maxTDP = new MaxTDPProperty(device.GetMaxTDP(), this);
            tdp = new TDPProperty(initialTDP, null, this);
        }

        public override void Update()
        {
            base.Update();

            /*if (ryzenAdjHandle != IntPtr.Zero)
            {
                Logger.Info($"get_core_clk={RyzenAdj.get_core_clk(ryzenAdjHandle, 0)} get_core_power={RyzenAdj.get_core_power(ryzenAdjHandle, 0)} get_fclk={RyzenAdj.get_fclk(ryzenAdjHandle)} get_gfx_clk={RyzenAdj.get_gfx_clk(ryzenAdjHandle)} get_soc_power={RyzenAdj.get_soc_power(ryzenAdjHandle)} get_socket_power={RyzenAdj.get_socket_power(ryzenAdjHandle)}");
                var setMaxResult = RyzenAdj.set_max_gfxclk_freq(ryzenAdjHandle, 2000);
                var setMinResult = RyzenAdj.set_min_gfxclk_freq(ryzenAdjHandle, 1000);
                //var nan2 = float.NaN;
                //var setResult = RyzenAdj.set_gfx_clk(ryzenAdjHandle, (uint)nan2);
                
                Logger.Info($"set_max={setMaxResult} set_min={setMinResult} set={"123"}");
            }*/

            foreach (var hardwareSensor in hardwareSensors)
            {
                hardwareSensor.Value = -1.0f;
            }
        }

        public int GetTDP()
        {
            if (ryzenAdjHandle == IntPtr.Zero)
            {
                Logger.Info("RyzenAdj not initialized");
                return 10;
            }

            RyzenAdj.refresh_table(ryzenAdjHandle);
            return (int)RyzenAdj.get_stapm_limit(ryzenAdjHandle);
        }

        public void SetTDP(int tdp)
        {
            if (ryzenAdjHandle == IntPtr.Zero)
            {
                Logger.Info("RyzenAdj not initialized");
                return;
            }
            //RyzenAdj.refresh_table(ryzenAdjHandle);
            RyzenAdj.set_fast_limit(ryzenAdjHandle, (uint)((tdp + 10) * 1000));
            RyzenAdj.set_slow_limit(ryzenAdjHandle, (uint)((tdp + 5) * 1000));
            RyzenAdj.set_stapm_limit(ryzenAdjHandle, (uint)(tdp * 1000));
#if DEBUG
            RyzenAdj.refresh_table(ryzenAdjHandle);
            Logger.Info($"Set TDP to {tdp}, current TDP is {RyzenAdj.get_fast_limit(ryzenAdjHandle)}");
#endif
        }
    }
}
