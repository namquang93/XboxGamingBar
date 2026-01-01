using System.Xml.Serialization;

namespace XboxGamingBarHelper.Settings
{
    [XmlRoot("Setting")]
    public struct Setting
    {
        [XmlElement("OnScreenDisplayProvider")]
        public int OnScreenDisplayProvider;

        [XmlElement("OnScreenDisplay")]
        public int OnScreenDisplay;

        [XmlArray("LosslessScalingShortcut")]
        [XmlArrayItem("Key")]
        public System.Collections.Generic.List<int> LosslessScalingShortcut;

        public Setting(int onScreenDisplayProvider, int onScreenDisplay)
        {
            OnScreenDisplayProvider = onScreenDisplayProvider;
            OnScreenDisplay = onScreenDisplay;
            LosslessScalingShortcut = new System.Collections.Generic.List<int>();
        }
    }
}
