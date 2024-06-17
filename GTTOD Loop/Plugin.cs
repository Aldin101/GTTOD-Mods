using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;
using K8Lib.Settings;

namespace loop
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("K8Lib")]
    public class Loop : BaseUnityPlugin
    {
        private ConfigEntry<bool> loopEnabled;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded");

            loopEnabled = Config.Bind("Settings", "LoopEnabled", true, "Enable or disable the loop feature");
        }

        private void Update()
        {
            if (GameManager.GM == null) return;

            List<string> generatedRun = GameManager.GM.GetComponent<GTTOD_Manager>().GeneratedRun;
            if (generatedRun == null) return;

            if (loopEnabled.Value)
            {
                if (generatedRun.Contains("THE SUMMIT")) generatedRun.Remove("THE SUMMIT");
            } else
            {
                if (!generatedRun.Contains("THE SUMMIT")) generatedRun.Add("THE SUMMIT");

            }
        }

        private void Start()
        {
            new TitleBar("loopSettingsTitle", "LOOP");
            new CheckBox("loopSettingsToggle", "ENABLED", loopEnabled.Value, onEnableValueChange);
        }

        public void onEnableValueChange(bool value)
        {
            loopEnabled.Value = value;
            Config.Save();
        }
    }
}
