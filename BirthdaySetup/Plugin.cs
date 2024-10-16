using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;

namespace BirthdaySetup
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class BirthdaySetup : BaseUnityPlugin
    {
        // Config
        // private ConfigEntry<bool> configEntry;


        AssetBundle bundle;
        GameObject prefab;
        private void Start()
        {
            bundle = AssetBundle.LoadFromFile("mods\\birthday");
            prefab = bundle.LoadAsset<GameObject>("birthday");
            DontDestroyOnLoad(Instantiate(prefab));
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