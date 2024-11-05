using UnityEngine;
using BepInEx;
using K8Lib.Settings;
using System.Reflection;
using System.IO;
using BepInEx.Configuration;
using System.Collections.Generic;

namespace AlternateLogos
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInIncompatibility("OldLogo")]
    public class AlternateLogos : BaseUnityPlugin
    {
        AssetBundle bundle;
        Sprite currentLogo;

        ConfigEntry<int> logo;

        List<string> logos = new List<string>
        {
            "DEFAULT",
            "OLD LOGO",
            "PRIDE FLAG",
            "TRANS FLAG",
            "BISEXUAL FLAG",
            "ACE FLAG",
            "ANDREW",
            "IDAHO"
        };
        
        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");
            loadAssetBundle();

            logo = Config.Bind("General", "Logo", 0, "The name of the logo to use");
        }

        private void Start()
        {
            new TitleBar("LogoTitle", "ALTERNATE LOGOS");
            new DropDown("AlternateLogos", "LOGO", logos, logo.Value, (value) =>
            {
                logo.Value = value;
                currentLogo = bundle.LoadAsset<Sprite>(logos[value]);
                Config.Save();
            });

            currentLogo = bundle.LoadAsset<Sprite>(logos[logo.Value]);
        }

        private void Update()
        {
            if (!GameManager.GM) return;
            GameObject logoObject = GameManager.GM.GetComponent<GTTOD_MainMenu>().MenuGroup.transform.GetChild(0).GetChild(1).GetChild(0).gameObject;

            if (logoObject == null) return;

            UnityEngine.UI.Image logoImage = logoObject.GetComponent<UnityEngine.UI.Image>();

            if (logoImage == null) return;

            logoImage.sprite = currentLogo;

            if (logoImage.sprite == null)
            {
                loadAssetBundle();
                logoImage.sprite = currentLogo;
            }
        }

        private void OnDestroy()
        {
            if (bundle != null)
            {
                bundle.Unload(false);
            }
        }

        private void loadAssetBundle()
        {
            if (bundle != null)
            {
                bundle.Unload(false);
            }

            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("AlternateLogos.logos"))
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
                }
            }
        }
    }
}