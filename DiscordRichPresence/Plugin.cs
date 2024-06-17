using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;
using BepInEx.Logging;
using DiscordRPC;
using DiscordRPC.Logging;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using System.Globalization;

namespace GTTODRichPresence
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class GTTODRichPresence : BaseUnityPlugin
    {
        DiscordRpcClient client;
        float timeSinceLastUpdate = 0f;

        private void Awake()
        {
            client = new DiscordRpcClient("1251363523422982225");

            client.Logger = new ConsoleLogger() { Level = DiscordRPC.Logging.LogLevel.Warning };

            client.OnReady += (sender, e) =>
            {
                Debug.Log("Received Ready from user {0}" + e.User.Username);
            };

            client.OnPresenceUpdate += (sender, e) =>
            {
                Debug.Log("Received Update! {0}" + e.Presence);
            };

            client.Initialize();

            client.SetPresence(new RichPresence()
            {
                Details = "Loading in...",
                Assets = new Assets()
                {
                    LargeImageKey = "logo",
                    SmallImageKey = "enemies_killed"
                }
            });

            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");
        }

        private void OnDestroy()
        {
            client.Dispose();
        }

        private void Update()
        {
            if (GameManager.GM == null) return;

            timeSinceLastUpdate += Time.deltaTime;

            if (timeSinceLastUpdate >= 5f)
            {
                UpdateDiscordPresence();
                timeSinceLastUpdate = 0f;
            }
        }

        void UpdateDiscordPresence()
        {
            string details = "On the Main Menu";
            string state = "";
            string smallImageText = "GUP!";

            if (GameManager.GM.Player.activeInHierarchy)
            {
                string sceneName = SceneManager.GetActiveScene().name.ToLower();
                TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
                string titleCaseName = textInfo.ToTitleCase(sceneName);
                details = "Playing on " + titleCaseName;
                state = "Threat Level " + GameManager.GM.GetComponent<GTTOD_AIManager>().CurrentThreatLevel;
                smallImageText = GameManager.GM.GetComponent<GTTOD_Manager>().SessionKills + " Enemies Killed";
            }


            client.SetPresence(new RichPresence()
            {
                Details = details,
                State = state,
                Assets = new Assets()
                {
                    LargeImageKey = "logo",
                    SmallImageKey = "enemies_killed",
                    SmallImageText = smallImageText,
                }
            });
        }
    }
}