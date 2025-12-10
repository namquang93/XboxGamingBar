using Shared.Enums;

namespace XboxGamingBar.Data
{
    internal class IsForegroundProperty : WidgetProperty<bool>
    {
        public IsForegroundProperty() : base(true, null, Function.Foreground)
        {
        }

        // Controlled by the widget only, no need to sync from helper.
        public override bool ShouldSync()
        {
            //return base.ShouldSync();
            return false;
        }
    }
}
