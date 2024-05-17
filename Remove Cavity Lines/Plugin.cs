using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib.Settings;

namespace RemoveCavityLines
{
    [BepInDependency(K8Lib.PluginInfo.PLUGIN_GUID)]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class RemoveCavityLines : BaseUnityPlugin
    {
        // Config
        private ConfigEntry<bool> enabled;

        CavityBlitter main = null;
        CavityBlitter weapon = null;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            // Config
            enabled = Config.Bind("Settings", "enabled", true, "enables the plugin");
        }

        private void Update()
        {
            if (main == null || weapon == null)
            {
                CavityBlitter[] cavities = FindObjectsOfType<CavityBlitter>();
                if (cavities.Length < 2) return;
                main = cavities[0];
                weapon = cavities[1];
            }

            main.enabled = !enabled.Value;
            weapon.enabled = !enabled.Value;
        }

        private void Start()
        {
            new TitleBar("RemoveCavityLinestitle", "REMOVE CAVITY LINES");
            new CheckBox("RemoveCavityLinesenabled", "ENABLED", enabled.Value, onToggle);
        }
        
        public void onToggle(bool value)
        {
            enabled.Value = value;
            Config.Save();
        }
    }
}