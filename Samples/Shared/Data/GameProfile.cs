using NLog;
using Shared.Utilities;
using System.Xml.Serialization;

namespace Shared.Data
{
    [XmlRoot("GameProfile")]
    public struct GameProfile
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [XmlElement("GameId")]
        public GameId GameId;

        [XmlElement("Use")]
        private bool use;
        public bool Use
        {
            get { return use; }
            set
            {
                if (use != value)
                {
                    use = value;
                    Save();
                }
            }
        }

        [XmlElement("TDP")]
        private int tdp;
        public int TDP
        {
            get { return tdp; }
            set
            {
                if (tdp != value)
                {
                    tdp = value;
                    Save();
                }
            }
        }

        [XmlIgnore]
        public string Path;

        public GameProfile(string gameName, string gamePath, bool inUse, int inTDP, string inPath)
        {
            GameId = new GameId(gameName, gamePath);
            use = inUse;
            tdp = inTDP;
            Path = inPath;
        }

        public bool IsValid()
        {
            return GameId.IsValid();
        }

        public static bool operator ==(GameProfile g1, GameProfile g2)
        {
            if (ReferenceEquals(g1, g2))
                return true;

            if (ReferenceEquals(g1, null) || ReferenceEquals(g2, null))
                return false;

            return g1.GameId == g2.GameId;
        }

        public static bool operator !=(GameProfile p1, GameProfile p2)
        {
            return !(p1 == p2);
        }

        public override bool Equals(object obj)
        {
            if (obj is GameProfile other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return GameId.GetHashCode();
        }

        // Export to xml string.
        public override string ToString()
        {
            return XmlHelper.ToXMLString(this, true);
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(Path))
            {
                Logger.Warn($"Can't save profile {GameId.Name} due to empty path.");
                return;
            }

            XmlHelper.ToXMLFile(this, Path);
        }
    }
}
