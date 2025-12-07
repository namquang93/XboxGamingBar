using Shared.Data;
using Shared.Enums;
using XboxGamingBarHelper.Core;
using XboxGamingBarHelper.Windows;

namespace XboxGamingBarHelper.Systems
{
    internal class ResolutionProperty : HelperProperty<Resolution, SystemManager>
    {
        public int Width { get { return Value.Width; }}
        public int Height { get { return Value.Height; }}

        public ResolutionProperty(Resolution inValue, SystemManager inManager) : base(inValue, null, Function.Resolution, inManager)
        {
        }

        protected override void NotifyPropertyChanged(string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            Logger.Info($"Property changed to {Value}");

            User32.SetResolution(Value.Width, Value.Height);
        }
    }
}
