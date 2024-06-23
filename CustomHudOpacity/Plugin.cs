using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine.UI;
using K8Lib.Settings;

using Slider = K8Lib.Settings.Slider;
using System;

namespace CustomHudOpacity
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class CustomHudOpacity : BaseUnityPlugin
    {
        const float ORIGINAL_TRANSPARENCTY = 0.9961f;

        ConfigEntry<bool> enabled;
        ConfigEntry<float> opacity;

        List<Image> backgrounds = new List<Image>();

        float timeSinceLastUpdate = 0;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            enabled = Config.Bind("Settings", "Enabled", true, "Enable or disable the plugin.");
            opacity = Config.Bind("Settings", "Opacity", ORIGINAL_TRANSPARENCTY, "Set the opacity of the HUD. 0 is invisible, 1 is fully visible.");
        }

        private void Start()
        {
            new TitleBar("CustomHudOpacityTitle", "CUSTOM HUD OPACITY");
            new CheckBox("CustomHudOpacityEnabled", "ENABLED", true, onCheckBoxChanged);
            new Slider("CustomHudOpacitySlider", "HUD OPACITY", 0.01f, 1f, opacity.Value, false, onSliderChanged);
        }

        private void Update()
        {
            if (GameManager.GM == null) return;

            timeSinceLastUpdate += Time.deltaTime;

            if (timeSinceLastUpdate > 1)
            {
                backgrounds.Clear();
                timeSinceLastUpdate = 0;
            }

            if (backgrounds.Count == 0)
            {
                Image[] images = FindObjectsOfType<Image>();
                foreach (Image image in images)
                {
                    if (image.mainTexture.name == "InventoryBackground")
                    {
                        backgrounds.Add(image);
                    }
                }
            }


            if (backgrounds[0] == null || backgrounds.Count < 10)
            {
                backgrounds.Clear();
                return;
            }

            foreach (Image background in backgrounds)
            {
                Color color = background.color;

                if (enabled.Value)
                {
                    color.a = opacity.Value;
                } else
                {
                    color.a = ORIGINAL_TRANSPARENCTY;
                }
                background.color = color;
            }
        }
        void onCheckBoxChanged(bool value)
        {
            enabled.Value = value;
            Config.Save();
        }
        void onSliderChanged(float value)
        {
            opacity.Value = value;
            Config.Save();
        }
    }
}