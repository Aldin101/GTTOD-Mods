using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;
using LudeoSDK;

namespace NoLudeo
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class NoLudeo : BaseUnityPlugin
    {
        // Config
        // private ConfigEntry<bool> configEntry;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            // Harmony patching
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            // Config
            // configEntry = Config.Bind("Settings", "Key", true, "Description");
        }

        private void OnDestroy()
        {
            // Harmony unpatching
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.UnpatchSelf();
        }
    }

    [HarmonyPatch(typeof(LudeoManager), "Awake")]
    public class patchName
    {
        public static bool Prefix(LudeoManager __instance)
        {
            GameObject.Destroy(__instance.gameObject);
            return false;
        }
    }
}