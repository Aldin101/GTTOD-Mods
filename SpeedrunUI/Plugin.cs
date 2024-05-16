using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;
using System.Collections.Generic;
using UnityEngine.UI;

namespace SpeedrunUI
{
    [BepInDependency(K8Lib.PluginInfo.PLUGIN_GUID)]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class SpeedrunUI : BaseUnityPlugin
    {
        private ConfigEntry<bool> speedometer;
        private ConfigEntry<bool> timer;
        public ConfigEntry<bool> endOfLevelTimeNotif;
        private ConfigEntry<int> units;

        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.CurrentCulture;
            string region = culture.Name;
            if (region == "en-US")
            {
                units = Config.Bind("Settings", "Units", 0, "0 = mph, 1 = km/h");
            }
            else
            {
                units = Config.Bind("Settings", "Units", 1, "0 = mph, 1 = km/h");
            }

            speedometer = Config.Bind("Settings", "Speedometer", true, "Show speedometer");
            timer = Config.Bind("Settings", "Timer", true, "Show timer");
            endOfLevelTimeNotif = Config.Bind("Settings", "EndOfLevelTimeNotif", true, "Show time notification at the end of the level");

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }

        private void OnDestroy()
        {
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.UnpatchSelf();
        }

        private void LateUpdate()
        {
            if (!GameManager.GM) return;
            GTTOD_HUD hud = GameManager.GM.GetComponent<GTTOD_HUD>();
            addSettings();
            if (speedometer.Value)
            {
                hud.Speedometer.SetActive(true);

                float playerSpeed = GameManager.GM.Player.GetComponent<Rigidbody>().velocity.magnitude - 2.5f;
                if (playerSpeed < 0) {
                    ac_WallController wallController = GameManager.GM.Player.GetComponent<ac_CharacterController>().WallController;
                    FieldInfo forwardSpeed = typeof(ac_WallController).GetField("ForwardSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
                    playerSpeed = (float)forwardSpeed.GetValue(wallController);
                }

                float playerSpeedMph = playerSpeed;
                float playerSpeedKmh = playerSpeedMph * 1.60934f;

                if (units.Value == 0)
                {
                    hud.Speedometer.transform.GetChild(1).gameObject.GetComponent<Text>().text = playerSpeedMph.ToString("F0") + " MPH";
                }
                else
                {
                    hud.Speedometer.transform.GetChild(1).gameObject.GetComponent<Text>().text = playerSpeedKmh.ToString("F0") + " KM/H";
                }

                hud.Speedometer.transform.GetChild(2).gameObject.GetComponent<Image>().fillAmount = playerSpeed / 50;
                hud.Speedometer.transform.GetChild(3).gameObject.GetComponent<Image>().fillAmount = playerSpeed / 50;
            }
            else
            {
                hud.Speedometer.SetActive(false);
            }

            if (timer.Value)
            {
                hud.TimerObject.SetActive(true);
            } else
            {
                hud.TimerObject.SetActive(false);
            }

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (mode == LoadSceneMode.Additive) return;
                if (scene.name == "GTTOD" || scene.name == "DISCLAIMER") return;

            };
        }

        void addSettings()
        {
            new SettingsManager.SettingsElement.TitleBar("SpeedrunUI", "SPEEDRUN UI");
            new SettingsManager.SettingsElement.CheckBox("Speedometer", "ENABLE SPEEDOMETER", speedometer.Value, (value) => { speedometer.Value = value; Config.Save(); });
            new SettingsManager.SettingsElement.CheckBox("Timer", "ENABLE TIMER", timer.Value, (value) => { timer.Value = value; Config.Save(); });
            new SettingsManager.SettingsElement.CheckBox("EndOfLevelTimeNotif", "ENABLE END OF LEVEL TIME POPUP", endOfLevelTimeNotif.Value, (value) => { endOfLevelTimeNotif.Value = value; Config.Save(); });
            new SettingsManager.SettingsElement.DropDown("unitsDropDown", "UNITS", new List<string> { "MPH", "KM/H" }, units.Value, (value) => { units.Value = value; Config.Save(); });
        }
    }

    [HarmonyPatch(typeof(GTTOD_Level), "StartLevel")]
    public class patchName
    {
         public static void Postfix(GTTOD_Level __instance)
         {
            if (BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<SpeedrunUI>().endOfLevelTimeNotif.Value)
            {
                GameManager.GM.GetComponent<GTTOD_HUD>().CornerPopUp("LEVEL COMPLEATED IN " + GameManager.GM.GetComponent<GTTOD_HUD>().TimerText.GetComponent<Text>().text.Replace("s", "") + " SECONDS", 0);
            }

            FieldInfo elapsedTime = typeof(GTTOD_HUD).GetField("TimeElapsed", BindingFlags.NonPublic | BindingFlags.Instance);
            elapsedTime.SetValue(GameManager.GM.GetComponent<GTTOD_HUD>(), 0);
        }
     }
}