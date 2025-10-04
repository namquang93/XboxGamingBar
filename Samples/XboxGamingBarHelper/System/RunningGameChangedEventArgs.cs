using Shared.Data;

namespace XboxGamingBarHelper.System
{
    internal class RunningGameChangedEventArgs
    {
        public RunningGame OldRunningGame { get; }

        public RunningGame NewRunningGame { get; }

        internal RunningGameChangedEventArgs(RunningGame oldRunningGame, RunningGame newRunningGame)
        {
            OldRunningGame = oldRunningGame;
            NewRunningGame = newRunningGame;
        }
    }
}
