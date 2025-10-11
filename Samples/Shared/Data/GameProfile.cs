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
            return XmlHelper.ToXMLString(this, true);
        }
    }
}
