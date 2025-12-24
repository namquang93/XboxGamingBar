#if !STORE
using LibreHardwareMonitor.Hardware;
#endif
using NLog;
using System;
//using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Windows.ApplicationModel.AppService;
using XboxGamingBarHelper.Core;
using XboxGamingBarHelper.Hardware.Devices;
using XboxGamingBarHelper.Hardware.Sensors;
using XboxGamingBarHelper.Power;

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

#if !STORE
        private readonly Computer computer;
        private readonly IVisitor updateVisitor;
#endif
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

        private readonly TDPControlSupportProperty tdpControlSupport;
        public TDPControlSupportProperty TDPControlSupport
        {
            get { return tdpControlSupport; }
        }

        internal HardwareManager(AppServiceConnection connection) : base(connection)
        {
#if !STORE
            // Initialize the computer sensors
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsNetworkEnabled = true,
                IsStorageEnabled = true,
                IsBatteryEnabled = true,
            };
            updateVisitor = new UpdateVisitor();
            computer.Open();
            computer.Accept(updateVisitor);
#endif

            var cpuId = string.Empty;
            var mainboardId = string.Empty;

#if !STORE
            foreach (IHardware hardware in computer.Hardware)
            {
                var properties = string.Empty;
                if (hardware.Properties.Count > 0)
                {
                    foreach (var property in hardware.Properties)
                    {
                        properties = properties.Length == 0 ? $"{property.Key}:{property.Value}" : $"{properties}, {property.Key}:{property.Value}";
                    }
                }

                Logger.Info($"Found hardware {hardware.HardwareType}: Name={hardware.Name}, Type={hardware.HardwareType}, Id={hardware.Identifier}, Properties={properties}");
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    cpuId = hardware.Name;
                }

                if (hardware.HardwareType == HardwareType.Motherboard)
                {
                    mainboardId = hardware.Name;
                }
            }
#endif

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

            var initialTDP = 25;
#if !STORE
            ryzenAdjHandle = RyzenAdj.init_ryzenadj();
            if (ryzenAdjHandle == IntPtr.Zero)
            {
                Logger.Error("RyzenAdj initialized failed.");
                tdpControlSupport = new TDPControlSupportProperty(false, this);
            }
            else
            {
                RyzenAdj.refresh_table(ryzenAdjHandle);
                // RyzenAdj.set_fast_limit(ryzenAdjHandle, 30000);
                initialTDP = (int)RyzenAdj.get_fast_limit(ryzenAdjHandle);
                Logger.Info($"RyzenAdj initialized successfully at {initialTDP}W.");
                tdpControlSupport = new TDPControlSupportProperty(true, this);
            }
#else
            Logger.Info("RyzenAdj is disabled due to Microsoft Store restrictions.");
            tdpControlSupport = new TDPControlSupportProperty(false, this);
#endif
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

#if STORE
            var powerStatus = System.Windows.Forms.SystemInformation.PowerStatus;

            BatteryLevel.Value = powerStatus.BatteryLifePercent * 100;
            BatteryRemainingTime.Value = powerStatus.BatteryLifeRemaining;

            if (PowerManager.TryGetBatteryState(out var battery))
            {
                if (battery.Charging)
                {
                    BatteryDischargeRate.Value = -1.0f;
                    BatteryChargeRate.Value = battery.Rate / 1000.0f;
                }
                else
                {
                    BatteryDischargeRate.Value = battery.Rate / 1000.0f;
                    BatteryChargeRate.Value = -1.0f;
                }
            }
            else
            {
                Logger.Warn("Can't get battery charge/discharge rate.");
            }
#else
            if (computer == null)
                return;

            computer.Accept(updateVisitor);
            foreach (IHardware hardware in computer.Hardware)
            {
                foreach (ISensor sensor in hardware.Sensors)
                {
                    //Logger.Info("[2] Hardware {3} Sensor: {0}, value: {1}, type: {2}", sensor.Name, sensor.Value, sensor.SensorType.ToString(), hardware.Name);

                    HardwareSensor hardwareSensorFound = null;
                    foreach (var hardwareSensor in hardwareSensors)
                    {
                        if (hardwareSensor.HardwareType == hardware.HardwareType && hardwareSensor.SensorType == sensor.SensorType && hardwareSensor.SensorName == sensor.Name)
                        {
                            hardwareSensorFound = hardwareSensor;
                            break;
                        }
                    }
                    if (hardwareSensorFound != null)
                    {
                        hardwareSensorFound.Value = sensor.Value ?? -1;
                    }
                }
            }
#endif
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
