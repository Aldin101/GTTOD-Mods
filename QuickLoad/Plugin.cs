using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace QuickLoad
{
    [BepInDependency("NoLudeo")]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class QuickLoad : BaseUnityPlugin
    {
        public static AsyncOperation operation;
        public static bool hasLoaded = false;
        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }

        private void OnDestroy()
        {
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.UnpatchSelf();
        }
    }

    [HarmonyPatch(typeof(GTTOD_Disclaimer), "FetchSaveFiles")]
    public class GTTOD_Disclaimer_FetchSaveFiles_Patch
    {
        public static void Postfix(GTTOD_Disclaimer __instance)
        {
            if (QuickLoad.hasLoaded) return;

            Application.backgroundLoadingPriority = ThreadPriority.Low;
            QuickLoad.operation = SceneManager.LoadSceneAsync(1);
            QuickLoad.operation.allowSceneActivation = false;
        }
    }

    [HarmonyPatch(typeof(GTTOD_Disclaimer), "LoadScene")]
    public class GTTOD_Disclaimer_LoadScene_Patch
    {
        public static bool Prefix(GTTOD_Disclaimer __instance)
        {
            if (QuickLoad.hasLoaded) return true;
            if (QuickLoad.operation == null) return true;

            Application.backgroundLoadingPriority = ThreadPriority.Normal;
            QuickLoad.operation.allowSceneActivation = true;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            __instance.TipText.text = __instance.RandomTips[Random.Range(0, __instance.RandomTips.Count)];
            __instance.LoadingSong.volume = PlayerPrefsPlus.GetFloat("MusicVolume", 100f) / 2f;
            __instance.LoadingSong.Play();
            EventMediator.Instance.Publish<int>("GAME_DISCLAIMER_STARTED_LOAD_SCENE", 0);

            QuickLoad.hasLoaded = true;

            return false;
        }
    }
}