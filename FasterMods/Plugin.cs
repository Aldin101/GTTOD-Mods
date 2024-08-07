﻿using UnityEngine;
using BepInEx;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;

namespace FasterMods
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class FasterMods : BaseUnityPlugin
    {

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

    [HarmonyPatch(typeof(GTTOD_Manager), "AttemptRestart")]
    public class AttemptRestartPatch
    {
        public static bool Prefix(GTTOD_Manager __instance)
        {
            __instance.Restart();
            return false;
        }
    }


    [HarmonyPatch(typeof(GTTOD_ModManager), "UnpackMods")]
    public class ModManagerUnpackPatch
    {
        public static int modsLoaded;
        public static List<AssetBundle> modAssetBundles = new List<AssetBundle>();

        public static bool Prefix(GTTOD_ModManager __instance)
        {
            return false;
        }

        public static void Postfix(GTTOD_ModManager __instance)
        {
            modsLoaded = 0;
            foreach (var bundle in modAssetBundles)
            {
                bundle.Unload(true);
            }
            modAssetBundles.Clear();

            List<Coroutine> coroutines = new List<Coroutine>();
            foreach (GTTODMod Mod in __instance.Mods)
            {
                coroutines.Add(__instance.StartCoroutine(UnpackModAsync(__instance, Mod)));
            }

            __instance.StartCoroutine(WaitForAllMods(__instance, coroutines));
        }



        private static IEnumerator WaitForAllMods(GTTOD_ModManager instance, List<Coroutine> coroutines)
        {
            foreach (Coroutine coroutine in coroutines)
            {
                yield return coroutine;
            }

            var hudManager = AccessTools.Field(typeof(GTTOD_ModManager), "HUDManager").GetValue(instance);
            if (AllModsLoaded(instance.Mods))
            {
                GameManager.GM.GetComponent<GTTOD_HUD>().GlobalPopUp("MODS FULLY UNPACKED, RUNNING MODS", 27, 5f);
                var runMods = AccessTools.Method(typeof(GTTOD_ModManager), "RunMods");
                instance.StartCoroutine((IEnumerator)runMods.Invoke(instance, null));
            }
            else
            {
                GameManager.GM.GetComponent<GTTOD_HUD>().GlobalPopUp("1 OR MORE MODS FAILED TO LOAD, ATTEMPTING MOD EXECUTION ANYWAYS", 24, 5f);
                GameManager.GM.GetComponent<GTTOD_HUD>().GlobalPopUp("PLEASE CHECK YOUR INSTALLED WORKSHOP MODS", 0, 5f);
                var runMods = AccessTools.Method(typeof(GTTOD_ModManager), "RunMods");
                instance.StartCoroutine((IEnumerator)runMods.Invoke(instance, null));
            }
        }

        private static IEnumerator UnpackModAsync(GTTOD_ModManager instance, GTTODMod Mod)
        {
            var hudManager = AccessTools.Field(typeof(GTTOD_ModManager), "HUDManager").GetValue(instance);
            string Path = Mod.GTTODModPath;

            bool loadingFinished = false;

            AssetBundleCreateRequest BundleToLoadRequest = AssetBundle.LoadFromFileAsync(Path);
            
            BundleToLoadRequest.completed += (AsyncOperation operation) =>
            {
                if (BundleToLoadRequest.assetBundle == null)
                {
                    GameManager.GM.GetComponent<GTTOD_HUD>().GlobalPopUp("WORKSHOP MOD FAILED TO LOAD, PLEASE CHECK INSTALLED MODS!", 24, 5f);
                    modsLoaded++;
                    BundleToLoadRequest = null;
                }
                loadingFinished = true;
            };

            while (!loadingFinished)
            {
                yield return null;
            }

            if (BundleToLoadRequest == null)
            {
                yield break;
            }

            AssetBundle BundleToLoad = BundleToLoadRequest.assetBundle;
            if (BundleToLoad != null)
            {
                modAssetBundles.Add(BundleToLoad);
                string[] array = Path.Split('\\', (char)StringSplitOptions.None);
                AssetBundleRequest assetRequest = new AssetBundleRequest();
                assetRequest = BundleToLoad.LoadAssetAsync<GameObject>(array[array.Length - 1].ToString());

                while (!assetRequest.isDone)
                {
                    if (assetRequest.progress == 1)
                    {
                        modsLoaded++;
                        GameManager.GM.GetComponent<GTTOD_HUD>().GlobalPopUp($"MOD LOADING PROGRESS: {modsLoaded}/{instance.Mods.Count}", 27, 5f);
                        break;
                    }
                    yield return null;
                }
                
                yield return assetRequest;

                if (assetRequest.asset == null)
                {
                    GameManager.GM.GetComponent<GTTOD_HUD>().GlobalPopUp("WORKSHOP MOD FAILED TO LOAD, PLEASE CHECK INSTALLED MODS!", 24, 5f);
                    yield break;
                }

                GameObject gameObject = assetRequest.asset as GameObject;
                if (gameObject != null)
                {
                    Mod.GTTODModObject = UnityEngine.Object.Instantiate<GameObject>(gameObject, instance.ModObjectParent).GetComponent<GTTOD_Mod>();
                    Mod.GTTODModName = Mod.GTTODModObject.WorkshopName;
                    Mod.GTTODModDescription = Mod.GTTODModObject.WorkshopDescription;
                    Mod.GTTODModThumbnail = Mod.GTTODModObject.WorkshopThumbnail;
                    Mod.GTTODModSteamID = Mod.GTTODModObject.SteamID;
                }
            }
            yield break;
        }

        private static bool AllModsLoaded(List<GTTODMod> mods)
        {
            foreach (GTTODMod mod in mods)
            {
                if (mod.GTTODModObject == null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}