using NLog;
using Shared.Data;
using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace XboxGamingBarHelper.Profile
{
    internal static class ProfileManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string PROFILE_FOLDER_NAME = "profiles";
        private const string GLOBAL_PROFILE_NAME = "global";
        private const string GLOBAL_PROFILE_FILE_NAME = "global.xml";


        private static GameProfile GlobalProfile;
        private static GameProfile CurrentProfile;

        public static void Initialize()
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
        }

        public static string GetGameProfilesFolder()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PROFILE_FOLDER_NAME);
        }

        public static string GetGlobalProfilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, GLOBAL_PROFILE_FILE_NAME);
        }

        public static string GetGameProfilePath(GameProfileKey gameProfileKey)
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

        public static bool TryLoadGameProfile(GameProfileKey gameProfileKey, out GameProfile gameProfile)
        {
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

            gameProfile.ToFile(GetGameProfilePath(gameProfile.Key));
        }

        public static void UpdateProfileTDP()
        {

        }
    }
}
