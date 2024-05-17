using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;
using System.Collections.Generic;
using System.Collections;

namespace TripAndFall
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class TripAndFall : BaseUnityPlugin
    {
        private bool tripped = false;
        private Coroutine tripCheckCoroutine;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");
        }

        private void Update()
        {
            if (GameManager.GM == null) return;

            ac_CharacterController player = GameManager.GM.Player.GetComponent<ac_CharacterController>();
            ac_CutsceneManager cutscene = GameManager.GM.gameObject.GetComponent<ac_CutsceneManager>();

            if (!(player.CharacterGroundState == ac_CharacterController.GroundState.InAir || player.CharacterGroundState == ac_CharacterController.GroundState.Onwall ||
                   player.CharacterGroundState == ac_CharacterController.GroundState.Grounded))
            {
                if (tripped)
                {
                    Vector3 playerPos = player.transform.position;
                    Quaternion rotation = GameManager.GM.CameraManager.transform.rotation;
                    cutscene.PlayCutscene(21, playerPos, rotation);
                    tripped = false;
                }
            }

            if (tripCheckCoroutine == null)
            {
                tripCheckCoroutine = StartCoroutine(tripCheck());
            }
        }

        private IEnumerator tripCheck()
        {
            ac_CharacterController player = GameManager.GM.Player.GetComponent<ac_CharacterController>();
            ac_CutsceneManager cutscene = GameManager.GM.gameObject.GetComponent<ac_CutsceneManager>();

            while (GameManager.GM != null)
            {
                yield return new WaitForSeconds(.1f);
                if (!(player.CharacterGroundState == ac_CharacterController.GroundState.InAir || player.CharacterGroundState == ac_CharacterController.GroundState.Onwall))
                {
                    continue;
                }
                if (player.ForwardVelocity > 12 || player.HorizontalVelocity > 12)
                {
                    float speed = Mathf.Max(player.ForwardVelocity, player.HorizontalVelocity);
                    int chance = Mathf.Clamp((int)(speed * 100), 0, 500);

                    int random = Random.Range(0, chance);
                    if (random == 4 && !cutscene.PlayingScene)
                    {
                        tripped = true;
                    }
                }
            }

            tripCheckCoroutine = null;
            yield break;
        }
    }
}