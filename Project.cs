using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace KeybindLib
{
    [BepInPlugin(modGUID, modName, modVersion)]
    internal class KeybindLibBase : BaseUnityPlugin
    {
        public const string modGUID = "SpiralMods." + modName;
        private const string modName = "KeybindLib";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static KeybindLibBase Instance;

        private static ManualLogSource mls;

        void Awake()
        {
            Instance = this;
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo($"{modName} has loaded (ModVersion: {modVersion}, ModGUID: {modGUID})!");

            harmony.PatchAll(typeof(EventHandler));
            harmony.PatchAll(typeof(HudManager));
        }
    }
}