using NLog;
using RTSSSharedMemoryNET;
using System;
using System.IO;
using System.Linq;
using Shared.Data;
using XboxGamingBarHelper.Windows;
using XboxGamingBarHelper.Core;
using Windows.ApplicationModel.AppService;
using System.Collections.Generic;
using Shared.Utilities;
using Microsoft.Win32;

namespace XboxGamingBarHelper.Systems
{
    public delegate void ResumeFromSleepEventHandler(object sender);

    internal class SystemManager : Manager
    {
        private static readonly string[] IgnoredProcesses =
        {
            "rustdesk.exe",
            "anydesk.exe",
            "parsecd.exe",
            "unity.exe",
            "unrealeditor.exe",
            "eacefsubprocess.exe",
            "rider64.exe",
        };

        // Some games might not be detected by Xbox Game Bar, emulated games using RetroArch, MelonDS, Citra, etc.
        private static readonly string[] GameProcesses =
        {
            "azahar.exe",
            "cemu.exe",
            "citron.exe",
            "dolphin.exe",
            "duckstation-qt-x64-releaseltcg.exe",
            "duckstation.exe",
            "eden.exe",
            "melonds.exe",
            "pcsx2-qtx64.exe",
            "pcsx2-qt.exe",
            "pcsx2.exe",
            "ppssppwindows64.exe",
            "ppssppwindows.exe",
            "ppsspp.exe",
            "retroarch.exe",
            "rpcs3.exe",
            "ryujinx.exe",
            "scummvm.exe",
            "shadps4.exe",
            "vita3k.exe",
            "xemu.exe",
            "xenia_canary.exe",
        };

        private readonly RunningGameProperty runningGame;
        public RunningGameProperty RunningGame
        {
            get { return runningGame; }
        }

        private readonly RefreshRatesProperty refreshRates;
        public RefreshRatesProperty RefreshRates
        {
            get { return refreshRates; }
        }

        private readonly RefreshRateProperty refreshRate;
        public RefreshRateProperty RefreshRate
        {
            get { return refreshRate; }
        }

        private readonly ResolutionProperty resolution;
        public ResolutionProperty Resolution
        {
            get { return resolution; }
        }

        private readonly ResolutionsProperty resolutions;
        public ResolutionsProperty Resolutions
        {
            get { return resolutions; }
        }

        private readonly TrackedGameProperty trackedGame;
        public TrackedGameProperty TrackedGame
        {
            get { return trackedGame; }
        }

        private IReadOnlyDictionary<GameId, GameProfile> Profiles { get; }

        // Keep track to current opening windows to determine currently running game.
        private Dictionary<int, ProcessWindow> ProcessWindows { get; }
        private Dictionary<int, AppEntry> AppEntries { get; }

        public event ResumeFromSleepEventHandler ResumeFromSleep;

        public SystemManager(AppServiceConnection connection, IReadOnlyDictionary<GameId, GameProfile> profiles) : base(connection)
        {
            Logger.Info("Create process windows.");
            ProcessWindows = new Dictionary<int, ProcessWindow>();
            Logger.Info("Create app entries.");
            AppEntries = new Dictionary<int, AppEntry>();
            Logger.Info("Save profiles for detecting games.");
            Profiles = profiles;

            trackedGame = new TrackedGameProperty(this);
            Logger.Info("Check current running game.");
            runningGame = new RunningGameProperty(this);
            Logger.Info("Check supported refresh rates.");
            refreshRates = new RefreshRatesProperty(User32.GetSupportedRefreshRates(), this);
            Logger.Info("Check current refresh rate.");
            refreshRate = new RefreshRateProperty(User32.GetCurrentRefreshRate(), this);

            resolution = new ResolutionProperty(new Resolution(User32.GetCurrentResolution()), this);
            var resolutionList = User32.GetSupportedNativeResolutions().Select(res => new Resolution(res.width, res.height)).ToList();
            resolutions = new ResolutionsProperty(new Resolutions(resolutionList), this);
            foreach (var res in resolutions.Value.AvailableResolutions)
            {
                Logger.Info($"Supported native resolution: {res.Width}x{res.Height} vs {resolution.Width}x{resolution.Height}");
            }

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        private RunningGame GetRunningGame()
        {
            try
            {
                User32.GetOpenWindows(ProcessWindows);
            }
            catch (Exception e)
            {
                Logger.Warn($"Can't get open windows: {e.Message}, {ProcessWindows.Count} found.");
            }

            if (ProcessWindows.Count == 0)
            {
                Logger.Debug("There is not any opening window, so no game detected");
            }

            AppEntries.Clear();
            AppEntry[] appEntries = Array.Empty<AppEntry>();
            if (RTSSHelper.IsRunning())
            {
                try
                {
                    appEntries = OSD.GetAppEntries();
                }
                catch (Exception e)
                {
                    Logger.Error($"Can't connect to Rivatuner Statistics Server: {e}");
                }
            }
            else
            {
                Logger.Debug("Rivatuner Statistics Server is not running, can't determine current game.");
            }

            foreach (var appEntry in appEntries)
            {
                AppEntries[appEntry.ProcessId] = appEntry;
            }

            var possibleGames = new List<RunningGame>();
            if (ProcessWindows.Count > 0)
            {
                foreach (var processWindow in ProcessWindows)
                {
                    var processPath = processWindow.Value.Path;
                    var processExecutable = Path.GetFileName(processPath).ToLower();
                    if (IgnoredProcesses.Contains(processExecutable))
                    {
                        Logger.Debug($"Window {processWindow.Value.Path} is ignored");
                        continue;
                    }

                    if (trackedGame.IsValid() && trackedGame.DisplayName == processWindow.Value.Title)
                    {
                        Logger.Debug($"Found window \"{processWindow.Value.Title}\" running {(processWindow.Value.IsForeground ? "foreground" : "background")} process id {processWindow.Key} at path \"{processWindow.Value.Path}\" named \"{processWindow.Value.ProcessName}\" matches the xbox game bar widget app tracker target.");
                        possibleGames.Add(new RunningGame(processWindow.Value.ProcessId, processWindow.Value.Title, processWindow.Value.Path, trackedGame.AumId, 0, processWindow.Value.IsForeground));
                    }

                    if (Profiles.ContainsKey(new GameId(processWindow.Value.Title, processWindow.Value.Path)))
                    {
                        Logger.Debug($"Found window \"{processWindow.Value.Title}\" running {(processWindow.Value.IsForeground ? "foreground" : "background")} process id {processWindow.Key} at path \"{processWindow.Value.Path}\" named \"{processWindow.Value.ProcessName}\" has profile, use it.");
                        possibleGames.Add(new RunningGame(processWindow.Value.ProcessId, processWindow.Value.Title, processWindow.Value.Path, string.Empty, 0, processWindow.Value.IsForeground));
                        continue;
                    }

                    if (AppEntries.TryGetValue(processWindow.Value.ProcessId, out var appEntry) && appEntry.InstantaneousFrames > 0)
                    {
                        Logger.Debug($"Found window \"{processWindow.Value.Title}\" running {(processWindow.Value.IsForeground ? "foreground" : "background")} process id {processWindow.Key} at path \"{processWindow.Value.Path}\" named \"{processWindow.Value.ProcessName}\" has {appEntry.InstantaneousFrames} FPS, use it.");
                        possibleGames.Add(new RunningGame(processWindow.Value.ProcessId, processWindow.Value.Title, processWindow.Value.Path, string.Empty, appEntry.InstantaneousFrames, processWindow.Value.IsForeground));
                        continue;
                    }

                    if (GameProcesses.Contains(processExecutable))
                    {
                        Logger.Debug($"Found window \"{processWindow.Value.Title}\" running {(processWindow.Value.IsForeground ? "foreground" : "background")} process id {processWindow.Key} at path \"{processPath}\" named \"{processWindow.Value.ProcessName}\" in pre-defined list.");
                        possibleGames.Add(new RunningGame(processWindow.Value.ProcessId, processWindow.Value.Title, processPath, string.Empty, 0, processWindow.Value.IsForeground));
                        continue;
                    }

                    Logger.Debug($"Window \"{processWindow.Value.Title}\" at path {processWindow.Value.Path} doesn't have profile nor FPS.");
                }
            }
            
            //if (possibleGames.Count == 0 && trackedGame.IsValid())
            //{
            //    possibleGames.Add(new RunningGame(-1, trackedGame.DisplayName, string.Empty, trackedGame.AumId, 0, true));
            //}

            if (possibleGames.Count == 0)
            {
                Logger.Debug("Not found any game running.");
                return new RunningGame();
            }
            else if (possibleGames.Count == 1)
            {
                Logger.Debug($"Found single running game {possibleGames[0].GameId.Name}.");
                return possibleGames[0];
            }
            else
            {
                RunningGame highestFPSGame = new RunningGame();
                foreach (var possibleGame in possibleGames)
                {
                    if (possibleGame.IsForeground)
                    {
                        Logger.Debug($"Found foreground running game {possibleGames[0].GameId.Name} in multiple running game.");
                        return possibleGame;
                    }

                    if (!highestFPSGame.IsValid())
                    {
                        highestFPSGame = possibleGame;
                    }
                    else if (highestFPSGame.FPS <= possibleGame.FPS)
                    {
                        highestFPSGame = possibleGame;
                    }
                }

                Logger.Debug($"Found highest FPS ({highestFPSGame.FPS}) game {highestFPSGame.GameId.Name} in multiple games.");
                return highestFPSGame;
            }
        }

        public override void Update()
        {
            base.Update();

            var currentRunningGame = GetRunningGame();
            if (RunningGame != currentRunningGame)
            {
                if (currentRunningGame.GameId.IsValid())
                {
                    Logger.Info($"Detect new running game {currentRunningGame.GameId.Name}.");
                }
                else
                {
                    Logger.Info($"Running game {RunningGame.Value.GameId.Name} stopped.");
                }
                RunningGame.SetValue(currentRunningGame);
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    Logger.Info($"System resumed from sleep/hibernate at: {DateTime.Now}");
                    // Add your custom logic here to execute on wake-up
                    ResumeFromSleep?.Invoke(this);
                    break;
                case PowerModes.Suspend:
                    Logger.Info($"System is going to sleep/hibernate at: {DateTime.Now}");
                    // Add your custom logic here to execute before sleep
                    break;
                case PowerModes.StatusChange:
                    Logger.Info($"Power mode status change detected: {DateTime.Now}");
                    break;
            }
        }
    }
}
