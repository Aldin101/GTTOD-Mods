using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;

namespace WaterFix
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class WaterFix : BaseUnityPlugin
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

        private void Update()
        {
            //Code that runs every frame
        }
    }

    [HarmonyPatch(typeof(WaterVolume), "OnDestroy")]
    partial class WaterVolume_OnDestroy
    {
        static void Prefix(WaterVolume __instance)
        {
            GameManager.GM.Player.GetComponent<ac_CharacterController>().Swim(false);
        }
    }
}