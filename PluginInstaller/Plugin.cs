using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;
using System.IO;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Security.Cryptography;
using System.Collections;

namespace PluginInstaller
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class PluginInstaller : BaseUnityPlugin
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

    [HarmonyPatch(typeof(GTTOD_Disclaimer), "FetchSaveFiles")]
    public class GTTOD_Disclaimer_FetchSaveFiles_Patch
    {
        const int loaderModID = 70;
        static void Prefix()
        {
            string workshopPath = Path.Combine(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\")), "workshop\\content\\541200");

            if (!Directory.Exists(workshopPath))
            {
                Debug.LogError("Workshop path not found!");
                return;
            }
            
            if (!File.Exists(Path.Combine(workshopPath, loaderModID.ToString(), "Client.exe"))) return;
            if (!File.Exists("PluginInstallerConfig"))
            {
                Debug.Log("No config file found, starting installer");
                System.Diagnostics.Process.Start(Path.Combine(workshopPath, loaderModID.ToString(), "Client.exe"));
                Application.Quit();
            }

            PluginInstallerConfig config = JsonConvert.DeserializeObject<PluginInstallerConfig>(File.ReadAllText("PluginInstallerConfig"));

            if (config.lastGameFileHash != computeZipHash(Path.Combine(Environment.CurrentDirectory, "Get To The Orange Door.exe")))
            {
                Debug.Log("Game file hash changed, starting installer");
                System.Diagnostics.Process.Start(Path.Combine(workshopPath, loaderModID.ToString(), "Client.exe"), "uninstall");
                Application.Quit();
            }

            string[] files = Directory.GetFiles(workshopPath, "pluginpackage.zip", SearchOption.AllDirectories);
            List<int> modIds = new List<int>();
            foreach (string file in files)
            {
                int modId = int.Parse(Path.GetDirectoryName(file).Replace(workshopPath, "").Replace("\\", "").Replace("/", ""));
                modIds.Add(modId);
            }

            foreach (int modId in modIds)
            {
                if (!config.lastInstalledHash.ContainsKey(modId))
                {
                    if (!supportsCurrentGameVersion(modId)) continue;
                 
                    Debug.Log($"Mod {modId} not installed, starting installer");
                    
                    System.Diagnostics.Process.Start(Path.Combine(workshopPath, loaderModID.ToString(), "Client.exe"));
                    Application.Quit();
                }

                if (config.lastInstalledHash[modId] != computeZipHash(Path.Combine(workshopPath, modId.ToString(), "pluginpackage.zip")))
                {
                    Debug.Log($"Mod {modId} hash changed, starting installer");

                    System.Diagnostics.Process.Start(Path.Combine(workshopPath, loaderModID.ToString(), "Client.exe"));
                    Application.Quit();
                }
            }
        }

        static string computeZipHash(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }
            }
        }

        static bool supportsCurrentGameVersion(int modId)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get($"http://localhost:5000/Plugins/{modId}"))
            {
                var operation = webRequest.SendWebRequest();
                while (!operation.isDone) { }

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error downloading metadata for mod {modId}: {webRequest.error}");
                    return false;
                }
                else
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    PluginMetadata metadata = JsonConvert.DeserializeObject<PluginMetadata>(jsonResponse);

                    return metadata.approvedGameHashes.Contains(computeZipHash(Path.Combine(Environment.CurrentDirectory, "Get To The Orange Door.exe")));
                }
            }
        }
    }

    public class PluginMetadata
    {
        public string name;
        public int modId;
        public List<string> approvedZipHashes;
        public List<string> approvedGameHashes;
        public List<string> droppedFiles;
    }

    public class PluginInstallerConfig
    {
        public string lastGameFileHash;
        public List<string> installedFiles;
        public Dictionary<int, string> lastInstalledHash;
    }
}