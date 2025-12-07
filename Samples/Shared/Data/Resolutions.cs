using System.Collections.Generic;
using System.Xml.Serialization;

namespace Shared.Data
{
    [XmlRoot("Resolutions")]
    public struct Resolutions
    {
        [XmlElement("AvailableResolutions")]
        public List<Resolution> AvailableResolutions;

        public Resolutions(List<Resolution> availableResolutions)
        {
            AvailableResolutions = availableResolutions;
        }
    }
}
