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

        public Setting(int onScreenDisplayProvider, int onScreenDisplay)
        {
            OnScreenDisplayProvider = onScreenDisplayProvider;
            OnScreenDisplay = onScreenDisplay;
        }
    }
}
