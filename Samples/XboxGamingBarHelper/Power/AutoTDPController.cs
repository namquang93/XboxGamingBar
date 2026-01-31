using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.AppService;
using XboxGamingBarHelper.Core;
using XboxGamingBarHelper.Hardware;

namespace XboxGamingBarHelper.Power
{
    internal class AutoTDPController : IManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AppServiceConnection Connection { get; set; }

        private const int MIN_TDP = 5;
        private const int MAX_TDP = 30;
        private const int AVERAGE_TDP = 15;
        private const int GOOD_THRESHOLD = 3;
        private const int BEST_THRESHOLD = 0;
        private const int STABLE_NUM_RECORDED_FPS = 3;
        private const int IDLE_NO_FPS_NUM = 5;
        private const int UNKNOWN = -1;

        private bool isEnabled;
        private int targetFPS = 60;
        private int noFPSCount;
        private Dictionary<int, List<(int fps, long timestamp)>> fpsHistory = new Dictionary<int, List<(int fps, long timestamp)>>();
        private long lastTDPChangeTimestamp;
        private int delayTimeAfterChangingTDP;

        public AutoTDPEnabledProperty AutoTDPEnabled { get; private set; }
        public TargetFPSProperty TargetFPSProperty { get; private set; }

        public AutoTDPController(AppServiceConnection connection)
        {
            Connection = connection;
            AutoTDPEnabled = new AutoTDPEnabledProperty(false, null, this);
            TargetFPSProperty = new TargetFPSProperty(60, null, this);
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                if (!isEnabled)
                {
                    fpsHistory.Clear();
                    noFPSCount = 0;
                }
                Logger.Info($"Auto TDP {(isEnabled ? "Enabled" : "Disabled")}");
            }
        }

        public int TargetFPS
        {
            get { return targetFPS; }
            set
            {
                targetFPS = value;
                Logger.Info($"Auto TDP Target FPS set to {targetFPS}");
            }
        }


        public void Update(int currentFPS, HardwareManager hardwareManager)
        {
            if (!isEnabled) return;

            // Simple throttling based on delay set by previous actions
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() < lastTDPChangeTimestamp + delayTimeAfterChangingTDP)
            {
                return;
            }

            if (currentFPS == 0)
            {
                if (noFPSCount >= IDLE_NO_FPS_NUM)
                {
                    int currentTDP = hardwareManager.TDP.Value;
                    if (currentTDP != AVERAGE_TDP)
                    {
                        Logger.Info($"Not playing any game, set TDP limit to {AVERAGE_TDP}W.");
                        hardwareManager.TDP.SetValue(AVERAGE_TDP);
                    }
                    fpsHistory.Clear();
                }
                else
                {
                    // Logger.Debug("No FPS detected, wait a bit longer to confirm that game is closed.");
                    noFPSCount++;
                }
                return;
            }

            noFPSCount = 0;
            int currentTdp = hardwareManager.TDP.Value;
            long fpsTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            RecordFPS(currentTdp, currentFPS, fpsTimestamp);

            // Detailed debug logging
            int historyCount = FindRecordedFPSCount(currentTdp);
            Logger.Debug($"AutoTDP Update: CurrentFPS={currentFPS}, CurrentTDP={currentTdp}, TargetFPS={targetFPS}, HistoryCount={historyCount}");

            if (historyCount < STABLE_NUM_RECORDED_FPS)
            {
                Logger.Debug($"Waiting for stable FPS at {currentTdp}W... ({historyCount}/{STABLE_NUM_RECORDED_FPS} samples)");
                return;
            }

            (int currentTdpAverageFps, long currentTdpLastUpdateTime) = FindRecordedFPS(currentTdp);

            Logger.Debug($"Average FPS for {currentTdp}W is {currentTdpAverageFps} (Target: {targetFPS})");

            bool superWell = false;

            if (currentTdpAverageFps >= targetFPS - BEST_THRESHOLD)
            {
                superWell = true;
            }

            if (currentTdpAverageFps >= targetFPS - GOOD_THRESHOLD)
            {
                // Performance is good, try to lower TDP
                if (currentTdp <= MIN_TDP)
                {
                    Logger.Info($"FPS {currentTdpAverageFps} good at {currentTdp}W, but already at MIN TDP.");
                    delayTimeAfterChangingTDP = 10;
                    lastTDPChangeTimestamp = fpsTimestamp;
                }
                else
                {
                    (int lessTdpAverageFps, long lessTdpLastUpdateTime) = FindRecordedFPS(currentTdp - 1);

                    if (lessTdpAverageFps == UNKNOWN)
                    {
                        Logger.Info($"FPS {currentTdpAverageFps} good at {currentTdp}W. Trying to lower TDP to {currentTdp - 1}W (Unknown history).");
                        hardwareManager.TDP.SetValue(currentTdp - 1);
                        delayTimeAfterChangingTDP = 1;
                        lastTDPChangeTimestamp = fpsTimestamp;
                    }
                    else if (lessTdpAverageFps < targetFPS - GOOD_THRESHOLD)
                    {
                        // Historical data says lowering TDP will hurt performance too much
                        if (superWell)
                        {
                            if (fpsTimestamp - lessTdpLastUpdateTime >= 10)
                            {
                                Logger.Info($"FPS {currentTdpAverageFps} super good at {currentTdp}W. Retrying {currentTdp - 1}W after 10s.");
                                hardwareManager.TDP.SetValue(currentTdp - 1);
                                delayTimeAfterChangingTDP = 1;
                                lastTDPChangeTimestamp = fpsTimestamp;
                            }
                            else
                            {
                                Logger.Info($"FPS {currentTdpAverageFps} super good at {currentTdp}W. Wait before retrying {currentTdp - 1}W.");
                                delayTimeAfterChangingTDP = 10 - (int)(fpsTimestamp - lessTdpLastUpdateTime);
                                lastTDPChangeTimestamp = fpsTimestamp;
                            }
                        }
                        else
                        {
                            Logger.Info($"FPS {currentTdpAverageFps} good at {currentTdp}W. Keep it here because {currentTdp - 1}W history was bad ({lessTdpAverageFps}).");
                            delayTimeAfterChangingTDP = 5;
                            lastTDPChangeTimestamp = fpsTimestamp;
                        }
                    }
                    else
                    {
                        Logger.Info($"FPS {currentTdpAverageFps} good at {currentTdp}W. History says {currentTdp - 1}W is also good ({lessTdpAverageFps}). Lowering.");
                        hardwareManager.TDP.SetValue(currentTdp - 1);
                        delayTimeAfterChangingTDP = 1;
                        lastTDPChangeTimestamp = fpsTimestamp;
                    }
                }
            }
            else
            {
                // Performance needs improvement
                if (currentTdp >= MAX_TDP)
                {
                    Logger.Info($"FPS {currentTdpAverageFps} below target at {currentTdp}W, but already at MAX TDP.");
                    delayTimeAfterChangingTDP = 10;
                    lastTDPChangeTimestamp = fpsTimestamp;
                }
                else
                {
                    Logger.Info($"FPS {currentTdpAverageFps} below target at {currentTdp}W. Increasing TDP to {currentTdp + 1}W.");
                    hardwareManager.TDP.SetValue(currentTdp + 1);
                    delayTimeAfterChangingTDP = 1;
                    lastTDPChangeTimestamp = fpsTimestamp;
                }
            }
        }

        private void RecordFPS(int tdp, int fps, long timestamp)
        {
            if (!fpsHistory.ContainsKey(tdp))
            {
                fpsHistory[tdp] = new List<(int fps, long timestamp)>();
            }

            var list = fpsHistory[tdp];
            list.Add((fps, timestamp));

            // Keep only recent history
            if (list.Count > STABLE_NUM_RECORDED_FPS * 2)
            {
                list.RemoveAt(0);
            }

            var debugFPSHistory = string.Empty;
            foreach (var fpsHistoryItem in fpsHistory)
            {
                debugFPSHistory = $"{fpsHistoryItem.Key}:[{string.Join(',', fpsHistoryItem.Value.Select(item => item.fps))}]";
            }
            Logger.Info($"FPS History: {debugFPSHistory}");
        }

        private int FindRecordedFPSCount(int tdp)
        {
            if (fpsHistory.TryGetValue(tdp, out var list))
            {
                return list.Count;
            }
            return 0;
        }

        private (int averageFps, long lastUpdateTime) FindRecordedFPS(int tdp)
        {
            if (fpsHistory.TryGetValue(tdp, out var list) && list.Count > 0)
            {
                // Simple average of stored values
                double avg = list.Average(x => x.fps);
                long lastTime = list.Max(x => x.timestamp);
                return ((int)Math.Round(avg), lastTime);
            }
            return (UNKNOWN, 0);
        }

        public void Update()
        {
            // Do nothing, called manually with params from SystemManager/Program
        }
    }
}
