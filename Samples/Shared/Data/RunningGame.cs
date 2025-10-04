namespace Shared.Data
{
    using System.IO;
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

        public static bool operator ==(RunningGame g1, RunningGame g2)
        {
            if (ReferenceEquals(g1, g2))
                return true;

            if (ReferenceEquals(g1, null) || ReferenceEquals(g2, null))
                return false;

            return g1.Name == g2.Name && g1.Path == g2.Path;
        }

        public static bool operator !=(RunningGame p1, RunningGame p2)
        {
            return !(p1 == p2);
        }

        public override bool Equals(object obj)
        {
            if (obj is RunningGame other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Path.GetHashCode();
        }

        // Export to xml string.
        public override string ToString()
        {
            var serializer = new XmlSerializer(typeof(RunningGame));
            var writer = new StringWriter();
            serializer.Serialize(writer, this);
            var runningGameString = writer.ToString();
            return runningGameString;
        }

        // Import from xml string.
        public static RunningGame FromString(string xmlString)
        {
            var serializer = new XmlSerializer(typeof(RunningGame));
            var reader = new StringReader(xmlString);
            var runningGame = (RunningGame)serializer.Deserialize(reader);
            reader.Dispose();
            return runningGame;
        }
    }
}
