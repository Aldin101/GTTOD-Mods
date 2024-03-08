﻿using UnityEngine;
using BepInEx;
using K8Lib;
using System.Reflection;
using System.IO;

namespace OldLogo
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class OldLogo : BaseUnityPlugin
    {

        Sprite logo = null;

        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream fileStream = assembly.GetManifestResourceStream("OldLogo.logo.sprite");

            if (fileStream == null)
            {
                Logger.LogError("Failed to load resource stream");
                return;
            }

            byte[] fieData = new byte[fileStream.Length];
            fileStream.Read(fieData, 0, (int)fileStream.Length);
            fileStream.Close();

            string tempPath = Path.Combine(Application.temporaryCachePath, "logo.sprite");
            File.WriteAllBytes(tempPath, fieData);

            AssetBundle assetBundle = AssetBundle.LoadFromFile(tempPath);

            if (assetBundle == null)
            {
                Logger.LogError("Failed to load asset bundle");
                return;
            }

            logo = assetBundle.LoadAsset<Sprite>("logo.sprite");

            if (logo == null)
            {
                Logger.LogError("Failed to load sprite");
                return;
            }

            File.Delete(tempPath);
        }

        private void Update()
        {
            GameManager gameManger = GameManager.GM;
            if (gameManger == null) return;
            GameObject GTTOD = gameManger.gameObject;

            GameObject logoObject = GTTOD.GetComponent<GTTOD_MainMenu>().MenuGroup.transform.GetChild(0).GetChild(1).GetChild(0).gameObject;

            if (logoObject == null) return;

            UnityEngine.UI.Image logoImage = logoObject.GetComponent<UnityEngine.UI.Image>();

            if (logoImage == null)
            {
                Debug.LogError("logo image not found");
                return;
            }

            logoImage.sprite = logo;
        }
    }
}