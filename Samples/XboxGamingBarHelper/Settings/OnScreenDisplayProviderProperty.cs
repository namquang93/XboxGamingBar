using Shared.Enums;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Settings
{
    internal class OnScreenDisplayProviderProperty : HelperProperty<int, SettingsManager>
    {
        public OnScreenDisplayProviderProperty(int inValue, SettingsManager inManager) : base(inValue, null, Function.Settings_OnScreenDisplayProvider, inManager)
        {
        }

        protected override void NotifyPropertyChanged(string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            if (Value >= 0 && Value < Program.onScreenDisplayProviders.Count)
            {
                var newOnScreenDisplayProvider = Program.onScreenDisplayProviders[Value];
                Logger.Info($"Change On-Screen Display provider to item {Value}: {newOnScreenDisplayProvider.GetType().Name}");
                Program.onScreenDisplay.ChangeManager(newOnScreenDisplayProvider);
            }
        }
    }
}
