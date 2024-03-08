using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuickWaystation
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class QuickWaystation : BaseUnityPlugin
    {

        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }

        private void OnDestroy()
        {
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.UnpatchSelf();
        }
    }


    [HarmonyPatch(typeof(GTTOD_Manager), "LoadScene")]
    public class LoadScenePatch
    {
        public static void Prefix(GTTOD_Manager __instance, string SceneName, float NewTransitionSpeed, Color NewTransitionColor, ref float DelayTime)
        {
            if (DelayTime > 1f)
            {
                DelayTime = 1f;
            }
        }
    }

    [HarmonyPatch(typeof(GTTOD_Waystation), "WaystationLoad")]
    public class WaystationLoadPatch
    {
        public static bool Prefix(GTTOD_Waystation __instance)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(GTTOD_Waystation), "DoorCutscene")]
    public class WaystationDoorCutscenePatch
    {
        public static bool Prefix(GTTOD_Waystation __instance)
        {
            __instance.StartCoroutine(NewDoorCutscene(__instance));
            return false;
        }

        public static IEnumerator NewDoorCutscene(GTTOD_Waystation __instance)
        {
            ac_CutsceneManager cutsceneManager = GameObject.FindAnyObjectByType<ac_CutsceneManager>();
            FieldInfo CutscenePositionField = AccessTools.Field(typeof(GTTOD_Waystation), "CutscenePosition");
            Transform CutscenePosition = (Transform)CutscenePositionField.GetValue(__instance);

            cutsceneManager.PlayCutscene(10, CutscenePosition.position, CutscenePosition.rotation);
            __instance.WaystationAnimator.SetTrigger("Close");
            yield return new WaitForSeconds(3f);
            yield return new WaitUntil(() => SceneManager.GetActiveScene().isLoaded);
            __instance.WaystationAnimator.SetTrigger("Open");
        }
    }
}