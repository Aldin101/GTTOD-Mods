using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;
using System.IO;
using System.Reflection;
using UnityEngine.SceneManagement;
using K8Lib.Settings;

namespace OutlineOnlyWeapons
{
    [BepInDependency(K8Lib.PluginInfo.PLUGIN_GUID)]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class OutlineOnlyWeapons : BaseUnityPlugin
    {
        AssetBundle bundle;
        Shader shader;
        GameObject sway;
        bool hasApplied = false;

        ConfigEntry<bool> on;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            on = Config.Bind("Settings", "Enabled", true, "Enable the plugin");

            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("OutlineOnlyWeapons.shaderBundle"))
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

        private void Start()
        {
            new TitleBar("OutlineOnlyWeapons", "OUTLINE ONLY WEAPONS");
            new CheckBox("OOWEnabled", "ENABLED", on.Value, onCheckBoxChange);
        }

        void onCheckBoxChange(bool value)
        {
            on.Value = value;
            Config.Save();
        }

        private void loadShader()
        {
            SceneManager.LoadScene("reference", LoadSceneMode.Additive);
            shader = GameObject.Find("shaderReference").GetComponent<MeshRenderer>().materials[0].shader;
            shader.maximumLOD = 0;
        }

        private void OnDestroy()
        {
            bundle.Unload(false);
        }

        private void Update()
        {
            if (GameManager.GM == null) return;
            if (shader == null) loadShader();
            if (sway == null) sway = GameObject.Find("WeaponParent");

            if (!on.Value)
            {
                if (!hasApplied) return;
                shader = Shader.Find("Standard");
            }

            foreach (Renderer renderer in sway.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.GetType() == typeof(ParticleSystemRenderer)) continue;
                foreach (Material material in renderer.materials)
                {
                    material.shader = shader;
                }
                foreach (Material material in renderer.sharedMaterials)
                {
                    material.shader = shader;
                }
            }
            foreach (Renderer renderer in FindObjectOfType<ac_BodyController>().gameObject.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.GetType() == typeof(ParticleSystemRenderer)) continue;
                foreach (Material material in renderer.materials)
                {
                    material.shader = shader;
                }
                foreach (Material material in renderer.sharedMaterials)
                {
                    material.shader = shader;
                }
            }
            foreach (AmmoCounter counter in sway.GetComponentsInChildren<AmmoCounter>(true))
            {
                counter.gameObject.SetActive(!on.Value);
            }
            hasApplied = true;
            if (!on.Value) hasApplied = false;
        }
    }
}