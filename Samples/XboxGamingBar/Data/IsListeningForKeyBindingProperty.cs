using Shared.Enums;

namespace XboxGamingBar.Data
{
    internal class IsListeningForKeyBindingProperty : WidgetProperty<bool>
    {
        public IsListeningForKeyBindingProperty() : base(false, null, Function.IsListeningForKeyBinding)
        {
        }
    }
}
