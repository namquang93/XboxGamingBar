using NLog;
using RTSSSharedMemoryNET;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Shared.Data;
using XboxGamingBarHelper.Windows;

namespace XboxGamingBarHelper.System
{
    internal static class SystemManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly string[] IgnoredProcesses = 
        {
            "rustdesk.exe",
            "anydesk.exe",
            "parsecd.exe",
            "unity.exe",
            "unrealeditor.exe",
            "eacefsubprocess.exe",
            "rider64.exe"
        };

        private static int GetForegroundProcessId()
        {
            IntPtr foregroundWindowHandle = Win32.GetForegroundWindow();
            if (foregroundWindowHandle == IntPtr.Zero)
            {
                Logger.Error("Can't get foreground window.");
                return -1;
            }
            IntPtr activeAppProcessId;
            Win32.GetWindowThreadProcessId(foregroundWindowHandle, out activeAppProcessId);
            if (activeAppProcessId == IntPtr.Zero)
            {
                Logger.Error("Can't get active process id.");
                return -1;
            }

            return (int)activeAppProcessId;
        }

        private static string GetWindowTitle(int processId)
        {
            var process = Process.GetProcessById(processId);
            if (process == null)
            {
                Logger.Error($"Can't get process id {processId}");
                return string.Empty;
            }

            IntPtr windowHandle = process.MainWindowHandle;
            if (windowHandle == IntPtr.Zero)
            {
                Logger.Error($"Can't get process id {processId}'s window handle");
                return string.Empty;
            }

            var length = Win32.GetWindowTextLength(windowHandle);
            if (length == 0)
            {
                Logger.Error($"Process id {processId} doesn't have window title??");
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder(length + 1);
            Win32.GetWindowText(windowHandle, sb, sb.Capacity);

            return sb.ToString();
        }

        public static RunningGame GetRunningGame()
        {
            var appEntries = OSD.GetAppEntries();
            var runningGame = new RunningGame();
            if (appEntries == null || appEntries.Length == 0)
            {
                Logger.Debug("There is not any app running.");
                return runningGame;
            }

            var foregroundProcessId = GetForegroundProcessId();

            foreach (var appEntry in appEntries)
            {
                var appPath = appEntry.Name;
                // Only check D3D applications.
                if (appEntry.InstantaneousFrames <= 0)
                {
                    Logger.Debug($"Process {appPath} is not a game.");
                    continue;
                }
                
                // Ignore some unwanted processes.
                var appExecutableFileName = Path.GetFileName(appPath);
                if (IgnoredProcesses.Contains(appExecutableFileName.ToLower()))
                {
                    Logger.Debug($"Process {appPath} is ignored");
                    continue;
                }

                // Get game name either from window's title or from executable file name.
                var gameName = GetWindowTitle(appEntry.ProcessId);
                if (string.IsNullOrEmpty(gameName))
                {
                    gameName = appExecutableFileName.Replace(".exe", string.Empty);
                }

                // Check if it's the foreground process.
                if (appEntry.ProcessId == foregroundProcessId)
                {
                    Logger.Debug($"Found game {appPath} ({gameName}) running foreground");
                    return new RunningGame(appEntry.ProcessId, gameName, appPath, appEntry.InstantaneousFrames, true);
                }

                // If it's not the foreground process, use the application that has the highest frames-per-second.
                if (appEntry.InstantaneousFrames <= runningGame.FPS)
                {
                    Logger.Debug($"Found game {appPath} running under lower FPS that current running game {runningGame.Name}");
                    continue;
                }

                Logger.Debug($"Found game {appPath} ({gameName}) running background");
                runningGame = new RunningGame(appEntry.ProcessId, gameName, appPath, appEntry.InstantaneousFrames, false);
            }

            return runningGame;
        }
    }
}
