using System.IO;
using System.Xml.Serialization;

namespace Shared.Data
{
    [XmlRoot("GameProfile")]
    public struct GameProfile
    {
        [XmlElement("Key")]
        public GameProfileKey Key;

        [XmlElement("Use")]
        public bool Use;

        [XmlElement("TDP")]
        public int TDP;

        public GameProfile(string name, string path, bool use, int tdp)
        {
            Key = new GameProfileKey(name, path);
            Use = use;
            TDP = tdp;
        }

        public bool IsValid()
        {
            return Key.IsValid();
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
    }
}
