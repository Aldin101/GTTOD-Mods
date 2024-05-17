using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Newtonsoft.Json;
using K8Lib;
using Newtonsoft.Json.Linq;

namespace UpdateNotifications
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class UpdateNotifications : BaseUnityPlugin
    {
        private List<ModCategory> mods;
        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            try
            {
                WebClient client = new WebClient();
                string modList = client.DownloadString("https://gttodmods.vegapatch.net/mods.json");
                JObject root = JObject.Parse(modList);
                JArray modCategoriesArray = (JArray)root["mods"];

                mods = new List<ModCategory>();
                foreach (JArray modCategoryArray in modCategoriesArray)
                {
                    List<JToken> modCategoryList = modCategoryArray.ToList();
                    ModCategory modCategory = new ModCategory
                    {
                        Category = modCategoryList[0].ToString(),
                        Mods = modCategoryList.Skip(1).Select(m => m.ToObject<Mod>()).ToList()
                    };
                    mods.Add(modCategory);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to download mod list" + e.Message);
                Destroy(this);
            }
        }



        private void Update()
        {
            if (GameManager.GM == null) return;
            GTTOD_HUD hud = GameManager.GM.gameObject.GetComponent<GTTOD_HUD>();
            if (hud == null) return;

            int modCount = 0;
            int upToDateCount = 0;
            foreach (ModCategory category in mods)
            {
                foreach (Mod mod in category.Mods)
                {
                    if (File.Exists(mod.VersionInfoFile))
                    {
                        string version = FileVersionInfo.GetVersionInfo(mod.VersionInfoFile).FileVersion;
                        if (mod.Deprecated)
                        {
                            hud.GlobalPopUp(mod.Name.ToUpper() + " IS DEPRECATED! PLEASE UNINSTALL IT!", 20, 10f);
                        } else if (version != mod.Version)
                        {
                            hud.GlobalPopUp(mod.Name.ToUpper() + " IS OUT OF DATE! CURRENT VERSION: " + mod.Version + " YOUR VERSION: " + version, 20, 10f);
                        } else upToDateCount++;
                        modCount++;
                    }
                }
            }
            if (modCount == upToDateCount)
            {
                hud.GlobalPopUp(modCount + " PLUGINS LOADED!", 20, 10f);
            } else
            {
                hud.GlobalPopUp(modCount + " PLUGINS LOADED, " + (modCount - upToDateCount) + " OUT OF DATE!", 20, 10f);
            }
            Destroy(this);
        }
    }
    public class Mod
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Download { get; set; }
        public List<string> Dependencies { get; set; }
        public List<string> Files { get; set; }
        public string VersionInfoFile { get; set; }
        public string MoreInfoLink { get; set; }
        public bool Deprecated { get; set; }
    }

    public class ModCategory
    {
        public string Category { get; set; }
        public List<Mod> Mods { get; set; }
    }

    public class Root
    {
        public List<List<object>> Mods { get; set; }
    }
}