using System;
using HarmonyLib;
using UnityEngine;
using Thor;
using UnityEngine.Events;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BepInEx;
using Rewired;

namespace KeybindLib
{
    /// <summary>
    /// Manages keybind registration for custom mod keybinds.
    /// </summary>
    public static class KeybindManager
    {
        /// <summary>
        /// Limit what type of key press is needed to activate a keybind.
        /// </summary>
        public enum KeyPressType
        {
            /// <summary>Triggered once when the key is pressed.</summary>
            OnKeyDown=0,
            /// <summary>Triggered once when the key is let go.</summary>
            OnKeyUp=1,
            /// <summary>Triggered constantly while the key is down using game's Update tick loop.</summary>
            WhileKeyHeld=2
        }

        /// <summary>
        /// Limit what minimum game environment is needed for keybind press to be to activate.
        /// </summary>
        public enum KeyEnvironment
        {
            /// <summary>Do not limit to any environment</summary>
            Any = 0,
            /// <summary>Only permit on Title Screen</summary>
            TitleScreen = 1,
            /// <summary>Only permit while in game and in a menu</summary>
            InGameMenu = 2,
            /// <summary>Only permit while in game and not in a menu</summary>
            InGame = 3
        }

        /// <example>
        /// <code>
        /// KeybindManager.RegisterEvent("TestDeveloper.TestMod", "test_key", "Test Keybind", KeyCode.F,
        ///     KeybindManager.KeyEnvironment.Any, KeybindManager.KeyPressType.OnKeyDown)
        ///     .AddListener(() =>
        /// {
        ///     KeyCode currentKey = KeybindManager.GetKey("TestDeveloper.TestMod", "test_key");
        ///     Debug.Log($"{currentKey} was pressed!");
        /// });
        /// </code>
        /// </example>
        /// <param name="modId">The unique identifier of the mod registering the keybind.</param>
        /// <param name="keyId">The unique identifier for the keybind within the mod.</param>
        /// <param name="displayName">Human-readable name shown in keybind configuration menus.</param>
        /// <param name="defaultKey">The default key code for the keybind.</param>
        /// <param name="keyEnvironment">The game environment where the keybind is active.</param>
        /// <param name="keyPressType">When the keybind event should trigger.</param>
        /// <returns>A UnityEvent that can be used to detect a keybind press when the keybind conditions are met.</returns>
        public static UnityEvent RegisterEvent(string modId, string keyId, string displayName, KeyCode defaultKey, KeyEnvironment keyEnvironment, KeyPressType keyPressType)
        {
            EventHandler.defaultKeyCodes[(modId,keyId)] = defaultKey;
            if (!ConfigHandler.Has(modId, keyId))
            {
                ConfigHandler.Set(modId, keyId, defaultKey);
            }
            return EventHandler.Register(modId, keyId, displayName, keyEnvironment, keyPressType);
        }

        /// <summary>Removes a UnityEvent if it exists.</summary>
        /// <param name="modId">The unique identifier of the mod that registered the keybind.</param>
        /// <param name="keyId">The unique identifier for the keybind within the mod.</param>
        public static void UnregisterEvent(string modId, string keyId) => EventHandler.Unregister(modId, keyId);

        /// <summary>Gets raw key data if it exists.</summary>
        /// <param name="modId">The unique identifier of the mod.</param>
        /// <param name="keyId">The unique identifier for the keybind within the mod.</param>
        /// <returns>Null/Default values if event's not present or, if event's present, returns a quadruple with the values ( Key's Event, Display Name , Key's environment , Key's Press Type )</returns>
        public static (UnityEvent, string, KeyEnvironment, KeyPressType) GetKeyData(string modId, string keyId) => EventHandler.Get(modId, keyId);

        /// <summary>Gets a UnityEvent if it exists.</summary>
        /// <param name="modId">The unique identifier of the mod.</param>
        /// <param name="keyId">The unique identifier for the keybind within the mod.</param>
        /// <returns>Null if event's not present or, if event's present, returns a UnityEvent that can be used to detect a keybind press when the keybind conditions are met.</returns>
        public static UnityEvent GetEvent(string modId, string keyId) => EventHandler.Get(modId, keyId).Item1;

        /// <summary>Gets a keybind's display name if it exists.</summary>
        /// <param name="modId">The unique identifier of the mod.</param>
        /// <param name="keyId">The unique identifier for the keybind within the mod.</param>
        /// <returns>Null if event's not present or, if event's present, returns keybind's Human-readable name shown in keybind configuration menus.</returns>
        public static string GetDisplayName(string modId, string keyId) => EventHandler.Get(modId, keyId).Item2;

        /// <summary>Gets all active keybinds that are loaded.</summary>
        /// <returns>A dictionary in this format <code>{modID1: [modKey1,modKey2,etc...], modID2: [modKey1,etc...]}</code>.</returns>
        public static Dictionary<string,List<string>> GetAllActiveKeys() => EventHandler.GetAllActiveKeys();

        /// <summary>Gets all active keybinds for a mod that is loaded.</summary>
        /// <param name="modId">The unique identifier of the mod.</param>
        /// <returns>A list of keybinds loaded for the mod in this format <code>[modKey1,modKey2,etc...]</code>.</returns>
        public static List<string> GetModActiveKeys(string modId) => EventHandler.GetModActiveKeys(modId);

        /// <summary>Sets or replaces the value for a keybind in mod's custom keybind config.</summary>
        /// <param name="modId">The unique identifier of the mod.</param>
        /// <param name="keyId">The unique identifier for the keybind within the mod.</param>
        /// <param name="keyCode">The key code for the keybind.</param>
        public static void SetKey(string modId, string keyId, KeyCode keyCode) => ConfigHandler.Set(modId, keyId, keyCode);

        /// <summary>Gets the value for a keybind in mod's custom keybind config.</summary>
        /// <param name="modId">The unique identifier of the mod.</param>
        /// <param name="keyId">The unique identifier for the keybind within the mod.</param>
        /// <returns>KeyCode.None if key's not present or, if key's present, returns KeyCode for requested keybind.</returns>
        public static KeyCode GetKey(string modId, string keyId) => ConfigHandler.Get(modId, keyId);


        /// <summary>Gets the default value for a keybind in mod's custom keybind config.</summary>
        /// <param name="modId">The unique identifier of the mod.</param>
        /// <param name="keyId">The unique identifier for the keybind within the mod.</param>
        /// <returns>KeyCode.None if key's not present or, if key's present, returns default KeyCode for requested keybind.</returns>
        public static KeyCode GetDefaultKey(string modId, string keyId) => EventHandler.defaultKeyCodes.TryGetValue((modId, keyId), out var keyCode) ? keyCode : KeyCode.None;

        /// <summary>Returns if keybind exists.</summary>
        /// <param name="modId">The unique identifier of the mod.</param>
        /// <param name="keyId">The unique identifier for the keybind within the mod.</param>
        /// <returns>true or false for if the key exists.</returns>
        public static bool HasKey(string modId, string keyId) => ConfigHandler.Has(modId, keyId);

        /// <summary>Removes keybind if present.</summary>
        /// <param name="modId">The unique identifier of the mod.</param>
        /// <param name="keyId">The unique identifier for the keybind within the mod.</param>
        public static void RemoveKey(string modId, string keyId) => ConfigHandler.Remove(modId, keyId);
    }

    internal static class EventHandler
    {
        // { (modID, keyId): (UnityEvent, displayName, environmentRequirement, keyPressTypeRequirement) }
        internal static readonly Dictionary<(string, string), (UnityEvent, string, KeybindManager.KeyEnvironment, KeybindManager.KeyPressType)> KeyBindings = new Dictionary<(string, string), (UnityEvent, string, KeybindManager.KeyEnvironment, KeybindManager.KeyPressType)>();
        internal static readonly Dictionary<(string, string), KeyCode> defaultKeyCodes = new Dictionary<(string, string), KeyCode>();

        [HarmonyPatch(typeof(Game))]
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        internal static void Update()
        {
            foreach (var (modId, keyId) in KeyBindings.Keys)
            {
                string keyPattern = $"{modId}:{keyId}";
                if (!ConfigHandler.Has(modId, keyId))
                {
                    Debug.LogWarning($"{KeybindLibBase.modGUID}: Failed to find registered key `{keyPattern}`, resetting to last known default.");
                    ConfigHandler.Set(modId, keyId, defaultKeyCodes[(modId, keyId)]);
                    continue;
                }

                KeyCode keyCode = ConfigHandler.Get(modId, keyId);
                if (ReInput.controllers.Keyboard.GetKeyDown(keyCode)) { OnKeyPress(modId, keyId, KeybindManager.KeyPressType.OnKeyDown); }
                if (ReInput.controllers.Keyboard.GetKeyUp(keyCode)) { OnKeyPress(modId, keyId, KeybindManager.KeyPressType.OnKeyUp); }
                if (ReInput.controllers.Keyboard.GetKeyTimePressed(keyCode) != 0) { OnKeyPress(modId, keyId, KeybindManager.KeyPressType.WhileKeyHeld); }
            }
        }

        private static void OnKeyPress(string modId, string keyId, KeybindManager.KeyPressType keyPress)
        {
            if (!ConfigHandler.Has(modId,keyId)) return;
            var (unityEvent, _, keyEnvironment, keyPressType) = KeyBindings[(modId,keyId)];

            if (keyPressType != keyPress) return;
            switch(keyEnvironment)
            {
                case KeybindManager.KeyEnvironment.TitleScreen:
                {
                    if (Game.Instance.State != Game.GameState.None) return;
                    break;
                }
                case KeybindManager.KeyEnvironment.InGameMenu:
                {
                    if (Game.Instance.State != Game.GameState.Playing) return;
                    if (!Game.Instance.Simulation.IsPaused) return;
                    if (Game.Instance.Simulation.LoadingZone) return;
                    if (Game.Instance.Simulation.Zone.MovingRooms) return;
                    break;
                }
                case KeybindManager.KeyEnvironment.InGame:
                {
                    if (Game.Instance.State != Game.GameState.Playing) return;
                    if (Game.Instance.Simulation.IsPaused) return;
                    if (Game.Instance.Simulation.LoadingZone) return;
                    break;
                }
                default:
                case KeybindManager.KeyEnvironment.Any:
                {
                    break;
                }
            }

            unityEvent?.Invoke();
        }

        public static UnityEvent Register(string modId, string keyId, string displayName, KeybindManager.KeyEnvironment keyEnvironment, KeybindManager.KeyPressType keyPressType)
        {
            Debug.Log($"{KeybindLibBase.modGUID}: Registering key `{modId}:{keyId}`");
            if (!KeyBindings.ContainsKey((modId, keyId)))
            {
                KeyBindings[(modId,keyId)] = (new UnityEvent(), displayName, keyEnvironment, keyPressType);
            }
            else
            {
                var (unityEvent, _, _, _) = KeyBindings[(modId,keyId)];
                KeyBindings[(modId,keyId)] = (unityEvent, displayName, keyEnvironment, keyPressType);
            }
            return KeyBindings[(modId,keyId)].Item1;
        }

        public static void Unregister(string modId, string keyId)
        {
            if (!ConfigHandler.Has(modId, keyId)) return;

            KeyBindings[(modId,keyId)].Item1.RemoveAllListeners();
            KeyBindings.Remove((modId,keyId));
        }

        public static (UnityEvent, string, KeybindManager.KeyEnvironment, KeybindManager.KeyPressType) Get(string modId, string keyId)
        {
            return ConfigHandler.Has(modId,keyId) ? KeyBindings[(modId,keyId)] : (null, null, default, default);
        }
        public static Dictionary<string,List<string>> GetAllActiveKeys()
        {
            Dictionary<string,List<string>> activeKeys = new Dictionary<string, List<string>>();
            foreach (var (modId, _) in KeyBindings.Keys)
            {
                if (activeKeys.ContainsKey(modId)) continue;
                activeKeys[modId] = GetModActiveKeys(modId);
            }
            return activeKeys;
        }

        public static List<string> GetModActiveKeys(string modId)
        {
            List<string> activeKeys = new List<string>();
            foreach (var (keysModId, keyId) in KeyBindings.Keys)
            {
                if (keysModId != modId) continue;
                activeKeys.Add(keyId);
            }
            return activeKeys;
        }
    }

    internal static class ConfigHandler
    {
        private static readonly string keybindConfigDirPath = Path.Combine(Paths.ConfigPath, "KeybindLib");
        // {modID: (modConfigHash, {keyID:keyCode}) }
        private static readonly Dictionary<string, (string,Dictionary<string, KeyCode>)> getDict = new Dictionary<string, (string,Dictionary<string, KeyCode>)>();

        private static string GetFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(fileStream);
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in hashBytes)
                    {
                        sb.Append(b.ToString("x2"));
                    }
                    return sb.ToString();
                }
            }
        }

        public static bool Has(string modId, string keyId)
        {
            return KeyCode.None != Get(modId, keyId);
        }

        public static KeyCode Get(string modId, string keyId)
        {
            string keybindConfigPath = Path.Combine(keybindConfigDirPath, $"{modId}.cfg");
            Directory.CreateDirectory(keybindConfigDirPath);
            string fileHash = GetFileHash(keybindConfigPath);

            if (!EventHandler.KeyBindings.ContainsKey((modId,keyId)) || !File.Exists(keybindConfigPath)) return KeyCode.None;
            if (!getDict.ContainsKey(modId) || fileHash != getDict[modId].Item1)
            {
                BuildModGetDict(modId);
            }
            if (!getDict.TryGetValue(modId, out var fetchedKeyDict)) return KeyCode.None;

            return fetchedKeyDict.Item2.TryGetValue(keyId, out var keyCode) ? keyCode : KeyCode.None;
        }

        private static void BuildModGetDict(string modId)
        {
            string keybindConfigPath = Path.Combine(keybindConfigDirPath, $"{modId}.cfg");
            string fileHash = GetFileHash(keybindConfigPath);
            getDict[modId] = getDict.TryGetValue(modId, out var foundTuple) ? (fileHash,foundTuple.Item2) : (fileHash, new Dictionary<string, KeyCode>());

            string[] lines = File.ReadAllLines(keybindConfigPath);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue;

                string[] parts = line.Split('=');
                if (parts.Length != 2) continue;

                string lineKey = parts[0].Trim();
                string lineValue = parts[1].Trim();

                if (!EventHandler.KeyBindings.ContainsKey((modId,lineKey))) continue;
                if (!Enum.TryParse<KeyCode>(lineValue, out var keyCode)) continue;
                KeyCode previousKeyCode = getDict[modId].Item2.TryGetValue(lineKey, out var fetchedKeyCode) ? fetchedKeyCode : KeyCode.None;
                if (keyCode == previousKeyCode) continue;

                getDict[modId].Item2[lineKey] = keyCode;
            }
        }

        public static void Set(string modId, string keyId, KeyCode KeyCode)
        {
            string keybindConfigPath = Path.Combine(keybindConfigDirPath, $"{modId}.cfg");

            Directory.CreateDirectory(keybindConfigDirPath);

            List<string> lines = new List<string>();
            if (File.Exists(keybindConfigPath))
            {
                lines = new List<string>(File.ReadAllLines(keybindConfigPath));
            }

            bool found = false;
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue;

                string[] parts = line.Split('=');
                if (parts.Length == 2 && parts[0].Trim() == keyId)
                {
                    lines[i] = $"{keyId}={KeyCode}";
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                lines.Add($"{keyId}={KeyCode}");
            }

            File.WriteAllLines(keybindConfigPath, lines);
        }

        public static void Remove(string modId, string keyId)
        {
            string keybindConfigPath = Path.Combine(keybindConfigDirPath, $"{modId}.cfg");

            Directory.CreateDirectory(keybindConfigDirPath);

            List<string> lines = new List<string>();
            if (File.Exists(keybindConfigPath))
            {
                lines = new List<string>(File.ReadAllLines(keybindConfigPath));
            }

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue;

                string[] parts = line.Split('=');
                if (parts.Length != 2 || parts[0].Trim() != keyId) continue;

                lines.RemoveAt(i);
                break;
            }

            File.WriteAllLines(keybindConfigPath, lines);
        }
    }
}
