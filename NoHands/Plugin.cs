using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;

namespace NoHands
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class NoHands : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }

        private void OnDestroy()
        {
            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.UnpatchSelf();
        }
    }

    [HarmonyPatch(typeof(WeaponScript), "Draw")]
    public class WeaponScriptPatch
    {
        public static void Postfix(WeaponScript __instance)
        {
            SkinnedMeshRenderer[] renderers = GameObject.FindObjectsOfType<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                if (renderer.name == "KRVHand" || renderer.name == "KRV-Hands-V2")
                {
                    renderer.enabled = !GameObject.FindAnyObjectByType<GTTOD_UpgradesManager>().AttunementRunes[6].RuneActive;
                }
            }
        }
    }

    [HarmonyPatch(typeof(GTTOD_UpgradesManager), "SetAttunement")]
    public class UpgradesManagerPatch
    {
        public static void Postfix(GTTOD_UpgradesManager __instance, int AttunementIndex)
        {
            if (AttunementIndex == 6)
            {
                SkinnedMeshRenderer[] renderers = GameObject.FindObjectsOfType<SkinnedMeshRenderer>();

                foreach (SkinnedMeshRenderer renderer in renderers)
                {
                    if (renderer.name == "KRVHand" || renderer.name == "KRV-Hands-V2")
                    {
                        renderer.enabled = !GameObject.FindAnyObjectByType<GTTOD_UpgradesManager>().AttunementRunes[6].RuneActive;
                    }
                }
            }
        }
    }
}