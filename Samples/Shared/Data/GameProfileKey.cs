using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Shared.Data
{
    [XmlRoot("GameProfileKey")]
    public struct GameProfileKey
    {
        [XmlElement("Name")]
        public string Name;

        [XmlElement("Path")]
        public string Path;

        public GameProfileKey(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Name);
        }
    }
}
