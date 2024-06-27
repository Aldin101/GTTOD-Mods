using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;
using UnityEngine.Video;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

namespace RedditGoldGag
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class RedditGoldGag : BaseUnityPlugin
    {
        AssetBundle bundle;
        VideoClip topVideo;
        VideoClip mainVideo;
        VideoClip rolling;
        VideoClip finished;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream("RedditGoldGag.redditgoldgag"))
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

            topVideo = bundle.LoadAsset<VideoClip>("TopVideo");
            mainVideo = bundle.LoadAsset<VideoClip>("MainVideo");
            rolling = bundle.LoadAsset<VideoClip>("Rolling");
            finished = bundle.LoadAsset<VideoClip>("DoneRolling");

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(ReplaceGagVideo());
        }

        IEnumerator ReplaceGagVideo()
        {
            yield return new WaitForSeconds(0.5f);

            GTTOD_GrabAGun gag = FindObjectOfType<GTTOD_GrabAGun>();
            if (gag == null) yield break;

            gag.ScreenParent.transform.GetChild(0).GetComponent<VideoPlayer>().clip = mainVideo;
            gag.ScreenParent.transform.GetChild(1).GetComponent<VideoPlayer>().clip = topVideo;

            gag.ScreenParent.transform.GetChild(0).GetComponent<VideoPlayer>().Play();
            gag.ScreenParent.transform.GetChild(1).GetComponent<VideoPlayer>().Play();

            gag.IdleClip = mainVideo;
            gag.ProcessingClip = rolling;
            gag.FinishedClip = finished;

            yield break;
        }
    }
}