using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClickToCrashFix
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} has loaded");
        }

        private void OnDestroy()
        {
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.UnpatchSelf();
        }
    }

    [HarmonyPatch(typeof(GTTOD_Disclaimer), "LoadScene")]
    public static class RuntimePatch
    {
        static bool Prefix(GTTOD_Disclaimer __instance)
        {
            SceneManager.LoadSceneAsync(1);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            __instance.TipText.text = __instance.RandomTips[Random.Range(0, __instance.RandomTips.Count)];
            __instance.LoadingSong.volume = PlayerPrefsPlus.GetFloat("MusicVolume", 100f) / 2f;
            __instance.LoadingSong.Play();

            return false;
        }
    }

}
