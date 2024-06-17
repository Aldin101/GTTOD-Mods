using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace FreakyCaches
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class FreakyCaches : BaseUnityPlugin
    {
        void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(SetCacheNames());
        }
        
        IEnumerator SetCacheNames()
        {
            yield return new WaitForSeconds(1);

            foreach (GTTOD_AlphaCache cache in FindObjectsOfType<GTTOD_AlphaCache>())
            {
                cache.GetComponent<GTTOD_Interactable>().Settings.NameText = "𝓯𝓻𝓮𝓪𝓴𝔂 𝓬𝓪𝓬𝓱𝓮";
            }
        }
    }
}