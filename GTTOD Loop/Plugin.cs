using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;
using K8Lib.Settings;

namespace loop
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("K8Lib")]
    public class Plugin : BaseUnityPlugin
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

            GameObject GTTOD = GameObject.Find("GTTOD");
            if (GTTOD == null) return;

            GTTOD = GTTOD.transform.GetRoot().gameObject;

            List<Rune> runes = GTTOD.GetComponent<GTTOD_UpgradesManager>().AttunementRunes;
            if (runes == null) return;

            List<string> generatedRun = GTTOD.GetComponent<GTTOD_Manager>().GeneratedRun;
            if (generatedRun == null) return;

            if (loopEnabled.Value)
            {
                if (!runes[0].RuneActive)
                {
                    if (generatedRun.Count == 12)
                    {
                        generatedRun.RemoveAt(11);
                    }
                }
                else
                {
                    if (generatedRun.Count == 19)
                    {
                        generatedRun.RemoveAt(18);
                    }
                }
            } else
            {
                if (!runes[0].RuneActive)
                {
                    if (generatedRun.Count == 11)
                    {
                        generatedRun.Add("THE SUMMIT");
                    }
                }
                else
                {
                    if (generatedRun.Count == 18)
                    {
                        generatedRun.Add("THE SUMMIT");
                    }
                }
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
