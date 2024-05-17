using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib.Settings;

namespace DarkTheme
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("K8Lib")]
    public class DarkTheme : BaseUnityPlugin
    {
        private ConfigEntry<bool> darkThemeEnabled;

        private void Awake()
        {
            darkThemeEnabled = Config.Bind("Settings", "darkThemeEnabled", true, "toggle the dark theme plugin");

            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            if (darkThemeEnabled.Value)
            {
                Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
                harmony.PatchAll();
            }
        }

        private void OnDestroy()
        {
            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.UnpatchSelf();
        }

        private void Start()
        {
             new TitleBar("DarkTheme", "DARK THEME");
             new CheckBox("DarkThemeCheckBox", "ENABLED", darkThemeEnabled.Value, onToggle);
        }

        public void onToggle(bool value)
        {
            if (value)
            {
                darkThemeEnabled.Value = true;
                Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
                harmony.UnpatchSelf();
                harmony.PatchAll();
            }
            else
            {
                darkThemeEnabled.Value = false;
                Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
                harmony.UnpatchSelf();
            }
            Config.Save();
        }
    }

    [HarmonyPatch(typeof(GTTOD_HUD), "Fade")]
    public class GTTOD_HUD_Fade_Patch
    {
        public static void Prefix(GTTOD_HUD __instance, ref float Amount, ref Color FadeColor)
        {
            if (FadeColor == Color.white) FadeColor = Color.black;
        }
    }

    [HarmonyPatch(typeof(GTTOD_Manager), "LoadScene")]
    public class GTTOD_Manager_LoadScene_Patch
    {
        public static void Prefix(GTTOD_Manager __instance, string SceneName, float NewTransitionSpeed, ref Color NewTransitionColor, float DelayTime)
        {
            if (NewTransitionColor == Color.white) NewTransitionColor = Color.black;
        }
    }
}