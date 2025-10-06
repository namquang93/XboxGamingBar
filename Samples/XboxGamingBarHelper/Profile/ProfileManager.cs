using NLog;
using Shared.Data;
using System;
using System.IO;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Profile
{
    internal delegate void OnProfileChanged(object sender, ProfileChangedEventArgs e);

    internal class ProfileManager : Manager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string PROFILE_FOLDER_NAME = "profiles";
        private const string GLOBAL_PROFILE_NAME = "global";
        private const string GLOBAL_PROFILE_FILE_NAME = "global.xml";

        public OnProfileChanged ProfileChanged;

        public GameProfile GlobalProfile { get; private set; }

        private GameProfile currentProfile;
        public GameProfile CurrentProfile
        {
            get { return currentProfile; }
            set
            {
                if (currentProfile != value)
                {
                    var oldProfile = currentProfile;
                    currentProfile = value;
                    ProfileChanged.Invoke(this, new ProfileChangedEventArgs(oldProfile, currentProfile));
                }
            }
        }

        public ProfileManager()
        {
            var gameProfilesFolder = GetGameProfilesFolder();
            if (!Directory.Exists(gameProfilesFolder))
            {
                Directory.CreateDirectory(gameProfilesFolder);
            }

            // Load global profile.
            var globalProfilePath = GetGlobalProfilePath();
            if (!File.Exists(globalProfilePath))
            {
                // Create global profile path when it's not previously exist.
                GlobalProfile = new GameProfile(GLOBAL_PROFILE_NAME, GLOBAL_PROFILE_NAME, true, 25);
                GlobalProfile.ToFile(globalProfilePath);
            }
            else
            {
                GlobalProfile = GameProfile.FromFile(globalProfilePath);
            }
            CurrentProfile = GlobalProfile;
        }

        public static string GetGameProfilesFolder()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PROFILE_FOLDER_NAME);
        }

        public static string GetGlobalProfilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, GLOBAL_PROFILE_FILE_NAME);
        }

        public static string GetGameProfilePath(GameId gameProfileKey)
        {
            return Path.Combine(GetGameProfilesFolder(), $"{Path.GetFileNameWithoutExtension(gameProfileKey.Path)}.xml");
        }

        /*public static bool HasGameProfile(string gameExecutablePath)
        {
            return File.Exists(Path.Combine(GetGameProfilesFolder(), Path.GetFileNameWithoutExtension(gameExecutablePath)));
        }

        public static bool HasGameProfile(GameProfileKey gameProfileKey)
        {
            return HasGameProfile(gameProfileKey.Path);
        }*/

        public static bool TryLoadGameProfile(GameId gameProfileKey, out GameProfile gameProfile)
        {
            if (!gameProfileKey.IsValid())
            {
                gameProfile = new GameProfile();
                return false;
            }
            gameProfile = GameProfile.FromFile(GetGameProfilePath(gameProfileKey));
            return gameProfile.IsValid();
        }

        public static void SaveGameProfile(GameProfile gameProfile)
        {
            if (!gameProfile.IsValid())
            {
                Logger.Info("Can't save invalid game profile.");
                return;
            }

            gameProfile.ToFile(GetGameProfilePath(gameProfile.GameId));
        }
    }
}
