namespace Shared.Data
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Contain information related to a running game.
    /// </summary>
    [Serializable]
    public struct RunningGame
    {
        public int ProcessId { get; }
        public string Name { get; }
        public string Path { get;}
        public uint FPS { get; }
        public bool IsForeground { get; }

        [JsonConstructor]
        public RunningGame(int processId, string name, string path, uint fps, bool isForeground)
        {
            ProcessId = processId;
            Name = name;
            Path = path;
            FPS = fps;
            IsForeground = isForeground;
        }

        public bool IsValid()
        {
            return ProcessId > 0;
        }
    }
}
