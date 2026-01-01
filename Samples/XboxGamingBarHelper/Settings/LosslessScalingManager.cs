using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NLog;
using global::Windows.System;

namespace XboxGamingBarHelper.Settings
{
    internal static class LosslessScalingManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Lossless Scaling", "Settings.xml");

        public static List<ushort> GetHotkey()
        {
            var keys = new List<ushort>();
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    Logger.Warn($"Lossless Scaling settings file not found at {SettingsPath}");
                    return keys;
                }

                var doc = XDocument.Load(SettingsPath);
                var root = doc.Root;
                if (root == null)
                {
                     Logger.Warn($"Lossless Scaling settings file {SettingsPath} is invalid or empty.");
                     return keys;
                }

                var hotkeyEl = root.Element("Hotkey");
                var modifiersEl = root.Element("HotkeyModifierKeys");

                string hotkeyStr = hotkeyEl?.Value;
                string modifiersStr = modifiersEl?.Value;

                // Parse Modifiers
                if (!string.IsNullOrEmpty(modifiersStr))
                {
                    var modifiers = modifiersStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var mod in modifiers)
                    {
                        if (ParseModifier(mod, out ushort vk))
                        {
                            keys.Add(vk);
                        }
                    }
                }

                // Parse Key
                if (!string.IsNullOrEmpty(hotkeyStr))
                {
                    if (InternalParseKey(hotkeyStr, out ushort vk))
                    {
                        keys.Add(vk);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error parsing Lossless Scaling settings.");
            }

            return keys;
        }

        private static bool ParseModifier(string mod, out ushort vk)
        {
            vk = 0;
            if (string.Equals(mod, "Control", StringComparison.OrdinalIgnoreCase)) vk = 0x11; // VK_CONTROL
            else if (string.Equals(mod, "Alt", StringComparison.OrdinalIgnoreCase)) vk = 0x12; // VK_MENU
            else if (string.Equals(mod, "Shift", StringComparison.OrdinalIgnoreCase)) vk = 0x10; // VK_SHIFT
            else if (string.Equals(mod, "Win", StringComparison.OrdinalIgnoreCase)) vk = 0x5B; // VK_LWIN
            
            return vk != 0;
        }

        private static bool InternalParseKey(string key, out ushort vk)
        {
            vk = 0;
            // Try parsing as VirtualKey enum
            if (Enum.TryParse(key, true, out VirtualKey virtualKey))
            {
                vk = (ushort)virtualKey;
                return true;
            }
            
            // Fallback for simple letters/numbers if enum parse fails (though VirtualKey usually handles A-Z, 0-9)
            if (key.Length == 1)
            {
                char c = char.ToUpperInvariant(key[0]);
                if (c >= 'A' && c <= 'Z')
                {
                    vk = (ushort)c; // ASCII matches VK for A-Z
                    return true;
                }
                if (c >= '0' && c <= '9')
                {
                    vk = (ushort)c; // ASCII matches VK for 0-9
                    return true;
                }
            }

            return false;
        }
    }
}
