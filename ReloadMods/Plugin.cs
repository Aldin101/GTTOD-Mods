using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib.Inventory;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ReloadMods
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(K8Lib.PluginInfo.PLUGIN_GUID)]
    public class ReloadMods : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");
        }

        void Start()
        {
            new InventoryIcon("ReloadModsButton", "RELOAD MODS", "RELOADS ALL WORKSHOP MODS, ANY UNSAVED PROGRESS WILL BE LOST", null, new Sprite(), ClickButton);
        }

        private void ClickButton()
        {
            foreach (AssetBundle bundle in AssetBundle.GetAllLoadedAssetBundles())
            {
                if (bundle.name.EndsWith(".ost") || bundle.name.EndsWith(".weapon") || bundle.name.EndsWith(".stage") || bundle.name.EndsWith(".thing"))
                {
                    bundle.Unload(true);
                }
            }
            DestroyImmediate(GTTOD_ModContainer.ModContainer.gameObject);
            SceneManager.LoadScene("GTTOD");
        }
    }
}