using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;

namespace NoLod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class NoLod : BaseUnityPlugin
    {
        private ConfigEntry<bool> enabled;
        private ConfigEntry<bool> summit;

        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            enabled = Config.Bind("Settings", "Enabled", true, "Is No LOD active and enabled?");
            summit = Config.Bind("Settings", "Summit", true, "Is No LOD active and enabled on the summit?");
        }

        private void Update()
        {
            if (enabled.Value)
            {
                if (SceneManager.GetActiveScene().name == "THE SUMMIT" && !summit.Value)
                {
                    K8stuff();
                    return;
                }
                GTTOD_LevelSegment[] segments = FindObjectsOfType<GTTOD_LevelSegment>();
                foreach (GTTOD_LevelSegment segment in segments)
                {
                    foreach (GameObject toggleObject in segment.ToggleObjects)
                    {
                        toggleObject.SetActive(true);
                    }
                }
            }
            K8stuff();
        }

        private void K8stuff()
        {
            new SettingsManager.SettingsElement.TitleBar("noLodTitle", "NO LOD");
            new SettingsManager.SettingsElement.CheckBox("noLodEnabled", "ENABLED", enabled.Value, onToggle);
            new SettingsManager.SettingsElement.CheckBox("noLodSummit", "ENABLED ON THE SUMMIT", summit.Value, onSummitToggle);
            //new ConsoleCommand("perftest", perfTest);
            new ConsoleCommand("pluginperftest", pluginPerfTest);
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
            GameObject bepInExManager = GameObject.Find("BepInEx_Manager");
            GameManager.GM.gameObject.GetComponent<GTTOD_HUD>().CenterPopUp("STARTING PERFORMANCE TEST", 20, 3f);

            FileStream fileStream = File.Open("pluginPerfTest.txt", FileMode.Create);
            StreamWriter writer = new StreamWriter(fileStream);
            writer.WriteLine("Starting test");

            // Get the baseline performance
            float baselinePerformance = 0f;
            yield return CalculateAverageFps(1f, averageFps => { baselinePerformance = averageFps; });
            writer.WriteLine($"Baseline Performance: {baselinePerformance}");

            // Loop through all components of bepinex manager and remove them one by one, writing the performance to a file 1 second after each removal.
            foreach (Component component in bepInExManager.GetComponents<Component>())
            {
                // Skip null, transform, K8Lib, and NoLod components
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
            yield return new WaitForSeconds(3f);
            off.Add((int)(1f / Time.deltaTime));
            enabled.Value = true;
            yield return new WaitForSeconds(1f);
            on.Add((int)(1f / Time.deltaTime));

            enabled.Value = false;
            SceneManager.LoadScene("FRACTURED RIDGE");
            yield return new WaitForSeconds(3f);
            off.Add((int)(1f / Time.deltaTime));
            enabled.Value = true;
            yield return new WaitForSeconds(1f);
            on.Add((int)(1f / Time.deltaTime));

            enabled.Value = false;
            SceneManager.LoadScene("THE SUMMIT");
            yield return new WaitForSeconds(3f);
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