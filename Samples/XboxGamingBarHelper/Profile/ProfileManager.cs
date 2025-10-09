using NLog;
using Shared.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.ApplicationModel.AppService;
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

        private OnProfileChanged profileChanged;
        public event OnProfileChanged ProfileChanged
        {
            add {
                if (value == null)
                {
                    Logger.Warn("Adding a null listener???");
                    return;
                }

                value?.Invoke(this, new ProfileChangedEventArgs(new GameProfile(), CurrentProfile));
                profileChanged += value;
            }
            remove {
                profileChanged -= value;
            }
        }

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
                    Logger.Info($"Profile changed from {oldProfile.GameId.Name} to {currentProfile.GameId.Name}");
                    profileChanged?.Invoke(this, new ProfileChangedEventArgs(oldProfile, currentProfile));
                }
            }
        }

        private Dictionary<GameId, GameProfile> gameProfiles;
        public IReadOnlyDictionary<GameId, GameProfile> GameProfiles
        {
            get { return gameProfiles; }
        }

        public ProfileManager(AppServiceConnection connection) : base(connection)
        {
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

            // Make sure game profiles folder is created.
            var gameProfilesFolder = GetGameProfilesFolder();
            if (!Directory.Exists(gameProfilesFolder))
            {
                Directory.CreateDirectory(gameProfilesFolder);
            }

            // Read all existing game profiles.
            var xmlFiles = Directory.GetFiles(gameProfilesFolder, "*.xml");
            var serializer = new XmlSerializer(typeof(GameProfile));
            gameProfiles = new Dictionary<GameId, GameProfile>();

            foreach (string filePath in xmlFiles)
            {
                Logger.Info($"Reading file: {Path.GetFileName(filePath)}");

                try
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        var data = (GameProfile)serializer.Deserialize(fileStream);
                        gameProfiles.Add(data.GameId, data);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading or deserializing XML file '{filePath}': {ex.Message}");
                }
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

        public static string GetGameProfilePath(GameId gameId)
        {
            return Path.Combine(GetGameProfilesFolder(), $"{Path.GetFileNameWithoutExtension(gameId.Path)}.xml");
        }

        public bool HasGameProfile(GameId gameId)
        {
            return gameProfiles.ContainsKey(gameId);
        }

        public static bool TryLoadGameProfile(GameId gameid, out GameProfile gameProfile)
        {
            if (!gameid.IsValid())
            {
                gameProfile = new GameProfile();
                return false;
            }
            gameProfile = GameProfile.FromFile(GetGameProfilePath(gameid));
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
