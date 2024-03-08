using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;

namespace NoHands
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class ModNameHere : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");
        }

        private void Update()
        {
            SkinnedMeshRenderer[] renderers = GameObject.FindObjectsOfType<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                if (renderer.name == "KRVHand" || renderer.name == "KRV-Hands-V2")
                {
                    renderer.enabled = !FindAnyObjectByType<GTTOD_UpgradesManager>().AttunementRunes[6].RuneActive;
                }
            }
        }
    }
}