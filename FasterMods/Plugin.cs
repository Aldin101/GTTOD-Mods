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

namespace FasterMods
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class FasterMods : BaseUnityPlugin
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

        public static bool Prefix(GTTOD_ModManager __instance)
        {
            // Disable the original method
            return false;
        }

        public static void Postfix(GTTOD_ModManager __instance)
        {
            modsLoaded = 0;

            // Unload all asset bundles
            AssetBundle.UnloadAllAssetBundles(true);
            // Start a separate coroutine for each mod
            List<Coroutine> coroutines = new List<Coroutine>();
            foreach (GTTODMod Mod in __instance.Mods)
            {
                coroutines.Add(__instance.StartCoroutine(UnpackModAsync(__instance, Mod)));
            }

            // Start a coroutine that waits for all the UnpackModAsync coroutines to finish
            __instance.StartCoroutine(WaitForAllMods(__instance, coroutines));
        }

        private static IEnumerator WaitForAllMods(GTTOD_ModManager instance, List<Coroutine> coroutines)
        {
            // Wait for all the UnpackModAsync coroutines to finish
            foreach (Coroutine coroutine in coroutines)
            {
                yield return coroutine;
            }

            // Check if all mods have been loaded
            var hudManager = AccessTools.Field(typeof(GTTOD_ModManager), "HUDManager").GetValue(instance);
            if (AllModsLoaded(instance.Mods))
            {
                AccessTools.Method(hudManager.GetType(), "GlobalPopUp").Invoke(hudManager, new object[] { "MODS FULLY UNPACKED, RUNNING MODS", 27, 5f });
                // Use AccessTools to get the RunMods method
                var runMods = AccessTools.Method(typeof(GTTOD_ModManager), "RunMods");
                // Use AccessTools to start the RunMods coroutine
                instance.StartCoroutine((IEnumerator)runMods.Invoke(instance, null));
            }
        }


        private static IEnumerator UnpackModAsync(GTTOD_ModManager instance, GTTODMod Mod)
        {
            var hudManager = AccessTools.Field(typeof(GTTOD_ModManager), "HUDManager").GetValue(instance);
            string Path = Mod.GTTODModPath;

            //AssetBundle BundleToLoad = AssetBundle.LoadFromFile(Path);

            AssetBundleCreateRequest BundleToLoadRequest = AssetBundle.LoadFromFileAsync(Path);
            yield return BundleToLoadRequest;

            AssetBundle BundleToLoad = BundleToLoadRequest.assetBundle;
            if (BundleToLoad != null)
            {
                string[] array = Path.Split('\\', (char)StringSplitOptions.None);

                //GameObject gameObject = BundleToLoad.LoadAsset<GameObject>(array[array.Length - 1].ToString());
                AssetBundleRequest assetRequest = BundleToLoad.LoadAssetAsync<GameObject>(array[array.Length - 1].ToString());
                BundleToLoad.LoadAssetAsync(array[array.Length - 1].ToString(), typeof(GameObject));

                while (!assetRequest.isDone)
                {
                    if (assetRequest.progress == 1)
                    {
                        modsLoaded++;
                        AccessTools.Method(hudManager.GetType(), "GlobalPopUp").Invoke(hudManager, new object[] { $"Mod loading progress: {modsLoaded}/{instance.Mods.Count}", 27, 5f });
                        break;
                    }
                    yield return null;
                }

                yield return assetRequest;

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