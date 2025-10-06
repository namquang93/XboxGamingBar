using NLog;
using System.IO;
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
        public bool Use;

        [XmlElement("TDP")]
        public int TDP;

        public GameProfile(GameId gameId, bool use, int tdp)
        {
            GameId = gameId;
            Use = use;
            TDP = tdp;
        }

        public GameProfile(string name, string path, bool use, int tdp)
        {
            GameId = new GameId(name, path);
            Use = use;
            TDP = tdp;
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
            var serializer = new XmlSerializer(typeof(GameProfile));
            var writer = new StringWriter();
            serializer.Serialize(writer, this);
            var gameProfileString = writer.ToString();
            return gameProfileString;
        }

        // Import from xml string.
        public static GameProfile FromString(string xmlString)
        {
            var serializer = new XmlSerializer(typeof(GameProfile));
            var reader = new StringReader(xmlString);
            var gameProfile = (GameProfile)serializer.Deserialize(reader);
            reader.Dispose();
            return gameProfile;
        }

        public static GameProfile FromFile(string path)
        {
            if (!File.Exists(path))
            {
                Logger.Info($"Game profile not found at that {path}");
                return new GameProfile();
            }

            var serializer = new XmlSerializer(typeof(GameProfile));
            var reader = new StreamReader(path);
            var gameProfile = (GameProfile)serializer.Deserialize(reader);
            reader.Close();
            reader.Dispose();

            return gameProfile;
        }

        public void ToFile(string path)
        {
            var serializer = new XmlSerializer(typeof(GameProfile));
            using (TextWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, this);
            }
            Logger.Info($"Save game profile {GameId.Name} to {path}");
        }
    }
}
