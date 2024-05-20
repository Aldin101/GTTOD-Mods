using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib.Settings;
using K8Lib.Commands;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;

namespace NoLod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(K8Lib.PluginInfo.PLUGIN_GUID)]
    public class NoLod : BaseUnityPlugin
    {
        private ConfigEntry<bool> enabled;
        private ConfigEntry<bool> summit;
        private string lastScene = "";
        GTTOD_LevelSegment[] segments = FindObjectsOfType<GTTOD_LevelSegment>();
        LODGroup[] groups = FindObjectsOfType<LODGroup>();

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            enabled = Config.Bind("Settings", "Enabled", true, "Is No LOD active and enabled?");
            summit = Config.Bind("Settings", "Summit", true, "Is No LOD active and enabled on the summit?");
        }

        private void Update()
        {
            if (GameManager.GM == null) return;
            if (enabled.Value)
            {
                if (SceneManager.GetActiveScene().name == "THE SUMMIT" && !summit.Value) return;

                if (lastScene != SceneManager.GetActiveScene().name)
                {
                    lastScene = SceneManager.GetActiveScene().name;
                    segments = FindObjectsOfType<GTTOD_LevelSegment>();
                    groups = FindObjectsOfType<LODGroup>();
                }

                if (segments != null)
                {
                    if (segments.Length != 0)
                    {
                        foreach (GTTOD_LevelSegment segment in segments)
                        {
                            if (segment == null) continue;
                            foreach (GameObject toggleObject in segment.ToggleObjects)
                            {
                                if (toggleObject == null) continue;
                                toggleObject.SetActive(true);
                            }
                        }
                    }
                }

                if (groups != null)
                {
                    if (groups.Length != 0)
                    {
                        foreach (LODGroup group in groups)
                        {
                            if (group == null) continue;
                            group.enabled = false;
                        }
                    }
                }
            }
        }

        private void Start()
        {
            new TitleBar("noLodTitle", "NO LOD");
            new CheckBox("noLodEnabled", "ENABLED", enabled.Value, onToggle);
            new CheckBox("noLodSummit", "ENABLED ON THE SUMMIT", summit.Value, onSummitToggle);
            new ConsoleCommand("lodtest", perfTest);
            new ConsoleCommand("plugintest", pluginPerfTest);
        }

        private void onToggle(bool value)
        {
            enabled.Value = value;
            Config.Save();
        }

        private void onSummitToggle(bool value)
        {
            summit.Value = value;
            Config.Save();
        }

        private void pluginPerfTest(string unsued)
        {
            StartCoroutine(pluginPerfTestCoroutine());
        }

        private IEnumerator pluginPerfTestCoroutine()
        {
            GameObject bepInExManager = BepInEx.Bootstrap.Chainloader.ManagerObject.gameObject;
            GameManager.GM.gameObject.GetComponent<GTTOD_HUD>().CenterPopUp("STARTING PERFORMANCE TEST", 20, 3f);

            FileStream fileStream = File.Open("pluginPerfTest.txt", FileMode.Create);
            StreamWriter writer = new StreamWriter(fileStream);
            writer.WriteLine("Starting test");

            float baselinePerformance = 0f;
            yield return CalculateAverageFps(1f, averageFps => { baselinePerformance = averageFps; });
            writer.WriteLine($"Baseline Performance: {baselinePerformance}");

            foreach (Component component in bepInExManager.GetComponents<Component>())
            {
                if (component == null || component is Transform || component is NoLod)
                {
                    continue;
                }
                if (component.GetType().Namespace == "K8Lib")
                {
                    continue;
                }
                writer.WriteLine($"Removing {component.GetType().Namespace}");
                UnityEngine.Object.Destroy(component);
                yield return new WaitForSeconds(2f);
                float newPerformance = 0f;
                yield return CalculateAverageFps(1f, averageFps => { newPerformance = averageFps; });
                writer.WriteLine($"Performance: {newPerformance}");
                writer.WriteLine($"Performance Difference: {newPerformance - baselinePerformance}");
                baselinePerformance = newPerformance;
            }

            writer.WriteLine("Test complete");
            writer.Close();
            fileStream.Close();

            GameManager.GM.gameObject.GetComponent<GTTOD_HUD>().CenterPopUp("FINISHED", 20, 3f);

            yield break;
        }

        private IEnumerator CalculateAverageFps(float duration, Action<float> callback)
        {
            float startTime = Time.time;
            float fpsSum = 0;
            int fpsCount = 0;

            while (Time.time < startTime + duration)
            {
                fpsSum += 1f / Time.deltaTime;
                fpsCount++;
                yield return null;
            }

            callback(fpsSum / fpsCount);
        }

        private void perfTest(string unsued)
        {
            StartCoroutine(perfTestCoroutine());
        }

        private IEnumerator perfTestCoroutine()
        {
            List<int> on = new List<int>();
            List<int> off = new List<int>();
            string startingScene = SceneManager.GetActiveScene().name;
            Vector3 playerPos = GameManager.GM.Player.transform.position;

            bool originalEnabled = enabled.Value;
            bool originalSummit = summit.Value;
            summit.Value = false;

            GameManager.GM.gameObject.GetComponent<GTTOD_HUD>().CenterPopUp("STARTING PERFORMANCE TEST", 20, 3f);

            enabled.Value = false;
            SceneManager.LoadScene("AMBER VALE");
            yield return new WaitForSeconds(5f);
            off.Add((int)(1f / Time.deltaTime));
            enabled.Value = true;
            yield return new WaitForSeconds(1f);
            on.Add((int)(1f / Time.deltaTime));

            enabled.Value = false;
            SceneManager.LoadScene("FRACTURED RIDGE");
            yield return new WaitForSeconds(5f);
            off.Add((int)(1f / Time.deltaTime));
            enabled.Value = true;
            yield return new WaitForSeconds(1f);
            on.Add((int)(1f / Time.deltaTime));

            enabled.Value = false;
            SceneManager.LoadScene("THE SUMMIT");
            yield return new WaitForSeconds(5f);
            off.Add((int)(1f / Time.deltaTime));
            enabled.Value = true;
            yield return new WaitForSeconds(1f);
            on.Add((int)(1f / Time.deltaTime));
            enabled.Value = false;

            SceneManager.LoadScene(startingScene);
            GameManager.GM.Player.transform.position = playerPos;

            List<string> resultStrings =
            [
                $"AMBER VALE: OFF({off[0]}) ON({on[0]})",
                $"FRACTURED RIDGE: OFF({off[1]}) ON({on[1]})",
                $"THE SUMMIT: OFF({off[2]}) ON({on[2]})",
            ];

            foreach (string result in resultStrings)
            {
                GameManager.GM.gameObject.GetComponent<GTTOD_HUD>().CenterPopUp(result, 20, 5f);
            }
            yield return new WaitForSeconds(5f);

            GTTOD_Waystation waystation = FindObjectOfType<GTTOD_Waystation>();
            waystation.gameObject.transform.position = Vector3.zero;

            enabled.Value = originalEnabled;
            summit.Value = originalSummit;

            yield return null;
        }
    }
}