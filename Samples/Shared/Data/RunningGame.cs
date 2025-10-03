namespace Shared.Data
{
    using System.Xml.Serialization;

    /// <summary>
    /// Contain information related to a running game.
    /// </summary>
    [XmlRoot("RunningGame")]
    public struct RunningGame
    {
        [XmlElement("ProcessId")]
        public int ProcessId;

        [XmlElement("Name")]
        public string Name;

        [XmlElement("Path")]
        public string Path;

        [XmlElement("FPS")]
        public uint FPS;

        [XmlElement("IsForeground")]
        public bool IsForeground;

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
