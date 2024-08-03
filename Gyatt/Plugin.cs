using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;
using System.IO;
using System.Reflection;

namespace Gyatt
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Gyatt : BaseUnityPlugin
    {

        private AssetBundle assetBundle;

        private void Awake()
        {

            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream("Gyatt.gyatt"))
            {
                if (stream == null)
                {
                    Logger.LogError("Failed to load gyatt asset bundle");
                    return;
                }

                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    assetBundle = AssetBundle.LoadFromMemory(memoryStream.ToArray());
                }
            }

            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

        }
        private void Update()
        {
            if (GameManager.GM == null) return;

            GameManager.GM.Player.GetComponent<GTTOD_HealthScript>().DeathObject = assetBundle.LoadAsset<GameObject>("Gyatt");
        }
    }

}