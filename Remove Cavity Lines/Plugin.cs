using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;

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
            Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            // Config
            enabled = Config.Bind("Settings", "enabled", true, "enables the plugin");
        }

        private void Update()
        {
            if (main == null || weapon == null)
            {
                CavityBlitter[] cavities = FindObjectsOfType<CavityBlitter>();
                main = cavities[0];
                weapon = cavities[1];
            }

            main.enabled = !enabled.Value;
            weapon.enabled = !enabled.Value;

            addSettings();
        }

        private void addSettings()
        {
            new SettingsManager.SettingsElement.TitleBar("RemoveCavityLinestitle", "REMOVE CAVITY LINES");
            new SettingsManager.SettingsElement.CheckBox("RemoveCavityLinesenabled", "ENABLED", enabled.Value, onToggle);
        }
        
        public void onToggle(bool value)
        {
            enabled.Value = value;
            Config.Save();
        }
    }
}