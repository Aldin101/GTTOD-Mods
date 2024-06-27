using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace LevelRemaining
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class LevelRemaining : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (GameManager.GM == null)
            {
                return;
            }

            int levelsLeft = GameManager.GM.GetComponent<GTTOD_Manager>().GeneratedRun.Count - GameManager.GM.GetComponent<GTTOD_Manager>().GeneratedRun.IndexOf(scene.name);
            GameManager.GM.GetComponent<GTTOD_HUD>().BigTextPopUp($"{levelsLeft} LEVELS LEFT", 0);
        }
    }
}