using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;

namespace ModNameHere
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class ModNameHere : BaseUnityPlugin
    {
        // Config
        // private ConfigEntry<bool> configEntry;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            // Harmony patching
            // var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            // harmony.PatchAll();

            // Config
            // configEntry = Config.Bind("Settings", "Key", true, "Description");
        }

        private void OnDestroy()
        {
            // Harmony unpatching
            // var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            // harmony.UnpatchSelf();
        }

        private void Update()
        {
            //Code that runs every frame
        }
    }

    // [HarmonyPatch(typeof(typeName), "methodName")]
    // public class patchName
    // {
    //     public static void Prefix(type __instance)
    //     {
    //         //code that runs before the original method
    //     }
    //     public static void Postfix(type __instance)
    //     {
    //         //code that runs after the original method
    //     }
    // }
}