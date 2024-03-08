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

        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            // Config
            enabled = Config.Bind("Settings", "enabled", true, "enables the plugin");
        }

        private void Update()
        {
            if (enabled.Value)
            {
                foreach (var cavity in FindObjectsOfType<CavityBlitter>())
                {
                    cavity.enabled = false;
                }
            }

            addSettings();
        }

        private void addSettings()
        {
            new SettingsManager.SettingsElement.TitleBar("RemoveCavityLinestitle", "REMOVE CAVITY LINES");
            new SettingsManager.SettingsElement.CheckBox("RemoveCavityLinesenabled", "ENABLED", enabled.Value, onToggle);
        }
        
        public void onToggle(bool value)
        {
            if (value == false)
            {
                GTTOD_HUD hud = FindObjectOfType<GTTOD_HUD>();
                hud.CenterPopUp("You need to restart the game for this change to take effect.", 20);
            }

            enabled.Value = value;
            Config.Save();
        }
    }
}