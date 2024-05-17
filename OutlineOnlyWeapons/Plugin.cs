using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;
using System.IO;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace OutlineOnlyWeapons
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class OutlineOnlyWeapons : BaseUnityPlugin
    {
        AssetBundle bundle;
        Shader shader;
        GameObject sway;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

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
            if (sway == null) sway = GameObject.Find("SwayParent");
            foreach (Renderer renderer in sway.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                foreach (Material material in renderer.materials)
                {
                    material.shader = shader;
                }
                foreach (Material material in renderer.sharedMaterials)
                {
                    material.shader = shader;
                }
            }
            foreach (MeshRenderer renderer in sway.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (Material material in renderer.materials)
                {
                    material.shader = shader;
                }
                foreach (Material material in renderer.sharedMaterials)
                {
                    material.shader = shader;
                }
            }
            foreach (AmmoCounter counter in sway.GetComponentsInChildren<AmmoCounter>())
            {
                counter.gameObject.SetActive(false);
            }
        }
    }
}