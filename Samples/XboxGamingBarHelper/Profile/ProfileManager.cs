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

        public static void Initialize()
        {
            var gameProfilesFolder = GetGameProfilesFolder();
            if (!Directory.Exists(gameProfilesFolder))
            {
                Directory.CreateDirectory(gameProfilesFolder);
            }
        }

        public static string GetGameProfilesFolder()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PROFILE_FOLDER_NAME);
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
            var gameProfilePath = GetGameProfilePath(gameProfileKey);
            if (!File.Exists(gameProfilePath))
            {
                Logger.Info($"Game profile not found at that {gameProfilePath}");
                gameProfile = new GameProfile();
                return false;
            }

            var serializer = new XmlSerializer(typeof(GameProfile));
            var reader = new StreamReader(gameProfilePath);
            gameProfile = (GameProfile)serializer.Deserialize(reader);
            reader.Close();
            reader.Dispose();
            return true;
        }

        public static void SaveGameProfile(GameProfile gameProfile)
        {
            var serializer = new XmlSerializer(typeof(GameProfile));
            var gameProfilePath = GetGameProfilePath(gameProfile.Key);
            using (TextWriter writer = new StreamWriter(gameProfilePath))
            {
                serializer.Serialize(writer, gameProfile);
            }
            Logger.Info($"Save game profile to {gameProfilePath}");
        }
    }
}
