using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using BepInEx;
using BepInEx.Configuration;
using K8Lib.Settings;

namespace three
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("K8Lib")]
    public class plugin : BaseUnityPlugin
    {
        public ConfigEntry<bool> enabled;
        public ConfigEntry<string> replacementString;
        private void Awake()
        {
            enabled = Config.Bind("Settings", "Enabled", true, "Enable the plugin");
            replacementString = Config.Bind("Settings", "Replacement String", ":3", "The text to replace all text with");

            Debug.Log($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");
            if (enabled.Value)
            {
                var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
                harmony.PatchAll();
            }
        }
        private void Start()
        {
            new TitleBar("3TitleBar", ":3");
            new CheckBox("3Enabled", "ENABLED", enabled.Value, onCheck);
            new TextInput("3ReplacementString", "REPLACEMENT STRING", replacementString.Value, "BLANK (removes ingame text)", onReplacementStringChange);
        }

        private void OnDestroy()
        {
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.UnpatchSelf();
        }
        private void Update()
        {
            if (!enabled.Value) return;

            Text[] allText = FindObjectsOfType<Text>();

            foreach (Text text in allText)
            {
                text.text = replacementString.Value;
            }

            GTTOD_InteractionManager_InteractionUpdate.replacementString = replacementString.Value;
        }


        public void onCheck(bool enabled)
        {
            this.enabled.Value = enabled;
            Config.Save();

            if (enabled)
            {
                var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
                harmony.PatchAll();
            }
            else
            {
                var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
                harmony.UnpatchSelf();
            }
        }

        public void onReplacementStringChange(string replacementString)
        {
            this.replacementString.Value = replacementString;
            Config.Save();
        }
    }
    [HarmonyPatch(typeof(GTTOD_InteractionManager), "InteractionUpdate")]
    public class GTTOD_InteractionManager_InteractionUpdate
    {
        public static string replacementString;
        public static void Postfix(GTTOD_InteractionManager __instance)
        {
            Text[] allText = GameObject.FindObjectsOfType<Text>();

            foreach (Text text in allText)
            {
                text.text = replacementString;
            }
        }
    }
}
