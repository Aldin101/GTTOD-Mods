using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib.Settings;
using System.Reflection;
using UnityStandardAssets.ImageEffects;

namespace WorldBlur
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class WorldBlur : BaseUnityPlugin
    {
        // Config
        // private ConfigEntry<bool> configEntry;

        public static GameObject blur;
        public static GameObject blurObject;
        public static ConfigEntry<bool> active;
        public static ConfigEntry<float> blurAmount;

        AssetBundle bundle;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            // Config
            active = Config.Bind("General", "Active", true, "Whether the mod is active or not");
            blurAmount = Config.Bind("General", "Blur Strength", 25f, "The amount of blur");

            // Load assets

            bundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("WorldBlur.worldblur"));
            blur = bundle.LoadAsset<GameObject>("WorldBlur");
        }

        private void Start()
        {
            new TitleBar("WorldBlur", "WORLD BLUR");
            new CheckBox("BlueActive", "ENABLED", active.Value, (value) =>
            {
                active.Value = value;
                Config.Save();
            });
            new Slider("BlurAmount", "BLUR STRENGTH", 0, 100, blurAmount.Value, true, (value) =>
            {
                blurAmount.Value = value;
                Config.Save();
            });
        }

        private void OnDestroy()
        {
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.UnpatchSelf();

            bundle.Unload(true);
        }
    }

    [HarmonyPatch(typeof(WeaponParent), "Step")]
    public class WeaponParent_Update_Patch
    {
        public static void Postfix(WeaponParent __instance)
        {
            if (WorldBlur.blurObject == null) WorldBlur.blurObject = GameObject.Instantiate(WorldBlur.blur, WorldBlur.blur.transform.position, WorldBlur.blur.transform.rotation, __instance.transform);
            WorldBlur.blurObject.SetActive(WorldBlur.active.Value);
            WorldBlur.blurObject.GetComponent<Renderer>().material.SetFloat("_blurSizeXY", WorldBlur.blurAmount.Value/25);
            
            if (WorldBlur.active.Value)
            {
                Camera.main.GetComponent<EdgeDetection>().enabled = false;
            } else if (PlayerPrefsPlus.GetBool("Outline"))
            {
                Camera.main.GetComponent<EdgeDetection>().enabled = true;
            }
        }
    }
}