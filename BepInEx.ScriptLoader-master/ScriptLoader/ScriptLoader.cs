using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Newtonsoft.Json;

namespace WorkshopScripts
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class WorkshopScripts : BaseUnityPlugin
    {
        public static Config config;

        private void Awake()
        {
            if (!Directory.Exists("WorkshopModDllCache"))
            {
                Directory.CreateDirectory("WorkshopModDllCache");
            }

            foreach (string file in Directory.GetFiles("WorkshopModDllCache"))
            {
                if (file.EndsWith(".fordeletion"))
                {
                    File.Delete(file);
                }
            }

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            var configFile = Path.Combine(Paths.ConfigPath, "WorkshopScripts.json");
            if (File.Exists(configFile))
            {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFile));
            }
            else
            {
                config = new Config();
                config.hasBeenChecked = new List<string>();
                config.lastModifyDates = new Dictionary<string, DateTime>();
            }
        }

        private void OnDestroy()
        {
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.UnpatchSelf();

            var configFile = Path.Combine(Paths.ConfigPath, "WorkshopScripts.json");
            File.WriteAllText(configFile, JsonConvert.SerializeObject(config));
        }
    }

    [HarmonyPatch(typeof(GTTOD_ModManager), "UnpackModAsync")]
    public class GTTOD_ModManager_UnpackModAsync_Patch
    {
        static void Prefix(GTTOD_ModManager __instance, GTTODMod Mod)
        {
            string modName = Path.GetFileNameWithoutExtension(Mod.GTTODModPath);

            if (File.Exists(Path.Combine("WorkshopModDllCache", modName + ".dll")))
            {
                if (!WorkshopScripts.config.lastModifyDates.ContainsKey(modName))
                {
                    WorkshopScripts.config.lastModifyDates.Add(modName, File.GetLastWriteTime(Mod.GTTODModPath));
                    if (WorkshopScripts.config.hasBeenChecked.Contains(modName))
                    {
                        WorkshopScripts.config.hasBeenChecked.Remove(modName);
                    }
                }
                if (WorkshopScripts.config.lastModifyDates[modName] == File.GetLastWriteTime(Mod.GTTODModPath))
                {
                    Assembly.LoadFrom(Path.Combine(Paths.GameRootPath, "WorkshopModDllCache", modName + ".dll"));
                    return;
                } else
                {
                    if (WorkshopScripts.config.hasBeenChecked.Contains(modName))
                    {
                        WorkshopScripts.config.hasBeenChecked.Remove(modName);
                    }
                }
            }

            if (WorkshopScripts.config.hasBeenChecked.Contains(modName))
            {
                return;
            }

            AssetBundle modBundle = AssetBundle.LoadFromFile(Mod.GTTODModPath);
            if (modBundle == null)
            {
                Debug.LogError("Failed to load mod bundle");
                return;
            }

            if (modBundle.GetAllAssetNames().Any(s => s.Contains(".scripts")))
            {
                GameManager.GM.GetComponent<GTTOD_HUD>().GlobalPopUp("COMPILING SCRIPTS FOR " + modName.ToUpper(), 0, 1.1f);

                Dictionary<string, byte[]> scripts = new Dictionary<string, byte[]>();

                foreach (var script in modBundle.LoadAllAssets<TextAsset>().Where(t => t.name.EndsWith(".cs")))
                {
                    byte[] data = Encoding.UTF8.GetBytes(script.text);
                    scripts.Add(script.name, data);
                }

                try
                {
                    Assembly assembly = MonoCompiler.Compile(scripts, null, modName);
                    if (assembly != null)
                    {
                        Assembly.LoadFrom(Path.Combine(Paths.GameRootPath, "WorkshopModDllCache", modName + ".dll"));

                        if (WorkshopScripts.config.lastModifyDates.ContainsKey(modName))
                        {
                            WorkshopScripts.config.lastModifyDates[modName] = File.GetLastWriteTime(Mod.GTTODModPath);
                        }
                        else
                        {
                            WorkshopScripts.config.lastModifyDates.Add(modName, File.GetLastWriteTime(Mod.GTTODModPath));
                        }
                        GameManager.GM.GetComponent<GTTOD_HUD>().GlobalPopUp("SCRIPTS COMPILED FOR " + modName.ToUpper(), 0, 5);
                    }
                    else
                    {
                        Debug.LogError("Failed to compile assembly");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error loading assembly: {ex.Message}");
                }
            }

            if (!WorkshopScripts.config.hasBeenChecked.Contains(modName))
            {
                WorkshopScripts.config.hasBeenChecked.Add(modName);
            }

            modBundle.Unload(true);
        }
    }

    public class Config
    {
        public List<string> hasBeenChecked;
        public Dictionary<string, DateTime> lastModifyDates;
    }
}
