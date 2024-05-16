using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;

namespace WonderousSpeedFix
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class WonderousSpeedFix : BaseUnityPlugin
    {

        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");
        }

        private void Update()
        {
            if (GameManager.GM == null) return;
            GameManager.GM.GetComponent<GTTOD_UpgradesManager>().AttunementRunes[5].RuneName = "RUNE OF WONDEROUS SPEED";
        }
    }
}