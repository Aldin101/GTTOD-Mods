using UnityEngine;
using BepInEx;
using K8Lib;
using System.Reflection;
using System.IO;
using UnityEngine.SceneManagement;

namespace TransFlagLogo
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInIncompatibility("OldLogo")]
    public class TransFlagLogo : BaseUnityPlugin
    {
        AssetBundle bundle = null;
        Sprite logo = null;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");
            loadAssetBundle();
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

            if (logoImage.sprite == null)
            {
                loadAssetBundle();
                logoImage.sprite = logo;
            }
        }

        private void loadAssetBundle()
        {
            if (bundle != null)
            {
                bundle.Unload(false);
            }

            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("TransFlagLogo.logo.sprite"))
            {
                if (stream == null)
                {
                    Logger.LogError("Failed to load asset bundle");
                    return;
                }

                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    bundle = AssetBundle.LoadFromMemory(memoryStream.ToArray());
                    logo = bundle.LoadAsset<Sprite>("logo.sprite");
                }
            }
        }
    }
}