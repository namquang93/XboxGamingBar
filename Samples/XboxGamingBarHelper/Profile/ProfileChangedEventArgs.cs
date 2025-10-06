using Shared.Data;

namespace XboxGamingBarHelper.Profile
{
    internal class ProfileChangedEventArgs
    {
        public GameProfile OldProfile { get; }

        public GameProfile NewProfile { get; }

        internal ProfileChangedEventArgs(GameProfile oldProfile, GameProfile newProfile)
        {
            OldProfile = oldProfile;
            NewProfile = newProfile;
        }
    }
}
