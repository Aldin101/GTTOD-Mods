using System.Collections;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib.Settings;

namespace AdaptiveIntractableGlow
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class AdaptiveIntractableGlow : BaseUnityPlugin
    {

        public ConfigEntry<bool> enabled;
        public ConfigEntry<bool> onlyYellowPart;
        public ConfigEntry<int> dimAmount;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            enabled = Config.Bind("Settings", "Enabled", true, "Enable the plugin");
            onlyYellowPart = Config.Bind("Settings", "OnlyYellowPart", false, "Only glow the yellow part of the interactable");
            dimAmount = Config.Bind("Settings", "DimAmount", 100, "Amount to dim the glow by");
        }

        private void Start()
        {
            new TitleBar("glowTitle", "ADAPTIVE INTRACTABLE GLOW");
            new CheckBox("glowEnabled", "ENABLED", enabled.Value, toggleEnabled);
            new CheckBox("glowOnlyYellowPart", "ONLY YELLOW PART", onlyYellowPart.Value, toggleOnlyYellowPart);
            new Slider("glowDimAmount", "DIM AMOUNT", 0, 100, dimAmount.Value, true, changeDimAmount);
        }

        private void OnDestroy()
        {
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.UnpatchSelf();
        }

        void toggleEnabled(bool value)
        {
            enabled.Value = value;
            Config.Save();
        }

        void toggleOnlyYellowPart(bool value)
        {
            onlyYellowPart.Value = value;
            Config.Save();
        }

        void changeDimAmount(float value)
        {
            dimAmount.Value = (int)value;
            Config.Save();
        }
    }

    [HarmonyPatch(typeof(GTTOD_Interactable), "DisableInteractable")]
    public class AdaptiveIntractableGlowPatch
    {
        public static void Prefix(GTTOD_Interactable __instance)
        {
            if (!BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<AdaptiveIntractableGlow>().enabled.Value) return;

            float dimAmount = BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<AdaptiveIntractableGlow>().dimAmount.Value / 100f;

            if (BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<AdaptiveIntractableGlow>().onlyYellowPart.Value)
            {
                foreach (var renderer in __instance.gameObject.transform.gameObject.GetComponentsInChildren<Renderer>())
                {
                    foreach (var material in renderer.materials)
                    {
                        Color color = material.GetColor("_EmissionColor");

                        material.SetColor("_EmissionColor", new Color(color.r * (1 - dimAmount), color.g * (1 - dimAmount), color.b * (1 - dimAmount)));
                    }
                }
            }
            else
            {
                foreach (var renderer in __instance.gameObject.transform.parent.gameObject.GetComponentsInChildren<Renderer>())
                {
                    foreach (var material in renderer.materials)
                    {
                        Color color = material.GetColor("_EmissionColor");

                        material.SetColor("_EmissionColor", new Color(color.r * (1 - dimAmount), color.g * (1 - dimAmount), color.b * (1 - dimAmount)));
                    }
                }
            }
        }
    }
}