using UnityEngine;
using BepInEx;
using System.Reflection;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HarmonyLib;

namespace X
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class X : BaseUnityPlugin
    {
        AssetBundle bundle = null;
        Sprite logo = null;
        Image logoImage;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");
            loadAssetBundle();

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }

        private void OnDestroy()
        {
            if (bundle != null)
            {
                bundle.Unload(true);
            }

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.UnpatchSelf();
        }

        private void Update()
        {
            if (GameManager.GM == null) return;

            GameObject logoImageObject = null;
            if (!logoImage) logoImageObject = GameObject.Find("TwitterIcon");
            if (logoImageObject && !logoImage) logoImage = logoImageObject.GetComponent<Image>();

            if (!logoImage) return;

            logoImage.sprite = logo;
        }

        private void loadAssetBundle()
        {
            if (bundle != null)
            {
                bundle.Unload(false);
            }

            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("X.x"))
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
                    logo = bundle.LoadAsset<Sprite>("x");
                }
            }
        }
    }

    [HarmonyPatch(typeof(GTTOD_MainMenu), "SetDescription")]
    public class GTTOD_MainMenu_SetDescription
    {
        public static void Postfix(GTTOD_MainMenu __instance, string Text)
        {
            if (Text == "TWITTER")
            {
                __instance.DescriptionText.text = "X";
            }
        }
    }
}