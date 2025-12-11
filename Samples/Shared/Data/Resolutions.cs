using NLog;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Shared.Data
{
    [XmlRoot("Resolutions")]
    public struct Resolutions : IEquatable<Resolutions>
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [XmlElement("AvailableResolutions")]
        public List<Resolution> AvailableResolutions;

        public Resolutions(Resolution availableResolution)
        {
            AvailableResolutions = new List<Resolution>
            {
                availableResolution
            };
        }

        public Resolutions((int width, int height) availableResolution)
        {
            AvailableResolutions = new List<Resolution>
            {
                new Resolution(availableResolution)
            };
        }

        public Resolutions(List<Resolution> availableResolutions)
        {
            AvailableResolutions = availableResolutions;
        }

        public Resolutions(List<(int width, int height)> resolutionPairs)
        {
            AvailableResolutions = new List<Resolution>();
            foreach (var pair in resolutionPairs)
            {
                AvailableResolutions.Add(new Resolution(pair));
            }
        }

        public static bool operator ==(Resolutions r1, Resolutions r2)
        {
            if (ReferenceEquals(r1, r2))
                return true;

            if (ReferenceEquals(r1, null) || ReferenceEquals(r2, null))
                return false;

            if ((r1.AvailableResolutions == null && r2.AvailableResolutions != null) ||
                (r1.AvailableResolutions != null && r2.AvailableResolutions == null))
                return false;

            if (r1.AvailableResolutions == null && r2.AvailableResolutions == null)
                return true;

            if (r1.AvailableResolutions.Count != r2.AvailableResolutions.Count)
                return false;

            for (int i = 0; i < r1.AvailableResolutions.Count; i++)
            {
                if (r1.AvailableResolutions[i] != r2.AvailableResolutions[i])
                    return false;
            }

            return true;
        }

        public static bool operator !=(Resolutions r1, Resolutions r2)
        {
            return !(r1 == r2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Resolutions other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            foreach (var res in AvailableResolutions)
            {
                hashCode ^= res.GetHashCode();
            }

            return hashCode;
        }

        public bool Equals(Resolutions other)
        {
            return this == other;
        }
    }
}
