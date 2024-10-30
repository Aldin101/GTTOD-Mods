using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;
using System.Reflection;

namespace NailgunFix
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class NailgunFix : BaseUnityPlugin
    {
        AssetBundle bundle;
        public static AudioClip nailgun;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            // Harmony patching
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            // Load the nailgun sound
            bundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("NailgunFix.nailgun"));
            nailgun = bundle.LoadAsset<AudioClip>("nailgun");
        }

        private void OnDestroy()
        {
            // Harmony unpatching
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.UnpatchSelf();
        }
    }

    [HarmonyPatch(typeof(GTTOD_OSTManager), "Awake")]
    public class GTTOD_OSTManager_Awake
    {
        public static void Postfix(GTTOD_OSTManager __instance)
        {
            __instance.OSTZones[9].ZoneTracks.Add(new GTTOD_OST
            {
                TrackName = "NAILGUN",
                TrackArtist = "AARON F. BIANCHI",
                TrackStart = NailgunFix.nailgun,
            });
        }
    }
}