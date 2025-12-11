using System;
using System.Xml.Serialization;

namespace Shared.Data
{
    [XmlRoot("Resolution")]
    public struct Resolution : IEquatable<Resolution>
    {
        [XmlElement("Width")]
        public int Width;

        [XmlElement("Height")]
        public int Height;

        public Resolution(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Resolution((int width, int height) pair)
        {
            Width = pair.width;
            Height = pair.height;
        }

        public static bool operator ==(Resolution r1, Resolution r2)
        {
            if (ReferenceEquals(r1, r2))
                return true;

            if (ReferenceEquals(r1, null) || ReferenceEquals(r2, null))
                return false;

            return r1.Width == r2.Width && r1.Height == r2.Height;
        }

        public static bool operator !=(Resolution g1, Resolution g2)
        {
            return !(g1 == g2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Resolution other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Width.GetHashCode() ^ Height.GetHashCode();
        }

        public override string ToString()
        {
            //return XmlHelper.ToXMLString(this, true);
            return $"{Width}x{Height}";
        }

        public bool Equals(Resolution other)
        {
            return this == other;
        }
    }
}
