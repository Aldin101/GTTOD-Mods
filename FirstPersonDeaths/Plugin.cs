using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace FirstPersonDeaths
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class FirstPersonDeaths : BaseUnityPlugin
    {
        // Config
        // private ConfigEntry<bool> configEntry;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            // Config
            // configEntry = Config.Bind("Settings", "Key", true, "Description");
        }

        private void OnDestroy()
        {
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.UnpatchSelf();
        }

        private void Update()
        {
            //Code that runs every frame
        }
    }

    [HarmonyPatch(typeof(GTTOD_DeathObject), "Start")]
    public class GTTOD_DeathObject_Start_Patch
    {
        public static bool dying = false;
        public static bool swimDeath = false;
        public static bool runUpdate = false;
        public static bool Prefix(GTTOD_DeathObject __instance)
        {
            runUpdate = false;
            __instance.StartCoroutine(playAnimations(__instance));

            return false;
        }
        public static IEnumerator playAnimations(GTTOD_DeathObject __instance)
        {
            dying = true;
            ac_CutsceneManager cutscene = GameManager.GM.gameObject.GetComponent<ac_CutsceneManager>();
            ac_CharacterController player = GameManager.GM.Player.GetComponent<ac_CharacterController>();
            GTTOD_Level level = GameObject.FindAnyObjectByType<GTTOD_Level>();

            __instance.Body.gameObject.SetActive(false);

            while (player.CharacterGroundState == ac_CharacterController.GroundState.InAir || player.CharacterGroundState == ac_CharacterController.GroundState.Onwall ||
                   player.CharacterGroundState == ac_CharacterController.GroundState.Grounded)
            {
                if (level != null)
                {
                    if (player.transform.position.y - 10 <= level.LevelResetPoint)
                    {
                        __instance.StartCoroutine(lookUpDeath(__instance));
                        yield break;
                    }
                }
                else
                {
                    int resetPoint = -90;
                    if (SceneManager.GetActiveScene().name == "DUNGEON") resetPoint = -70;
                    if (player.transform.position.y <= resetPoint)
                    {
                        __instance.StartCoroutine(lookUpDeath(__instance));
                        yield break;
                    }
                }
                player.ToggleFreezePlayer(false);
                yield return null;
            }

            if (player.CharacterGroundState == ac_CharacterController.GroundState.Swimming)
            {
                swimDeath = true;
                __instance.StartCoroutine(lookUpDeath(__instance));
                yield break;
            }

            if (player.ForwardVelocity > 6 || player.HorizontalVelocity > 6)
            {
                Vector3 playerPos = player.transform.position;
                Quaternion rotation = GameManager.GM.CameraManager.transform.rotation;

                cutscene.PlayCutscene(21, playerPos, rotation);
                yield return new WaitForSeconds(1.3f);

                while (__instance.Fade.alpha <= .99f)
                {
                    if (__instance.Fade.alpha + Time.deltaTime > 1f)
                    {
                        __instance.Fade.alpha = 1f;
                    }
                    else
                    {
                        __instance.Fade.alpha += Time.deltaTime;
                    }
                    yield return null;
                }
                yield return new WaitForFixedUpdate();
                
                Vector3 cutsceneStart = GameManager.GM.Player.gameObject.transform.position;
                cutsceneStart.y = playerPos.y + .5f;

                cutscene.EndCutscene(playerPos, 0, new Vector2(rotation.x, rotation.z));
                cutscene.PlayCutscene(2, cutsceneStart, rotation);
                yield return new WaitForEndOfFrame();
                AudioSource audioSource = GameObject.Find("Die(Clone)").GetComponent<AudioSource>();
                Animator animator = GameObject.Find("Die(Clone)").GetComponent<Animator>();
                animator.speed = 30;
                yield return new WaitForSeconds(.1f);
                animator.speed = 1f;
                audioSource.time = 2.6f;
                yield return new WaitForSeconds(.2f);
                while (__instance.Fade.alpha >= .6f)
                {
                    __instance.Fade.alpha -= Time.deltaTime;
                    yield return null;
                }


                while (__instance.Fade.alpha <= .99f)
                {
                    if (__instance.Fade.alpha + Time.deltaTime / 2 > 1f)
                    {
                        __instance.Fade.alpha = 1f;
                    }
                    else
                    {
                        __instance.Fade.alpha += Time.deltaTime / 2;
                    }
                    yield return null;
                }

                while (__instance.Fade.alpha >= .4f)
                {
                    __instance.Fade.alpha -= Time.deltaTime / 2;
                    yield return null;
                }

                while (__instance.Fade.alpha <= .8f)
                {
                    if (__instance.Fade.alpha + Time.deltaTime / 2 > .8f)
                    {
                        __instance.Fade.alpha = .8f;
                    }
                    else
                    {
                        __instance.Fade.alpha += Time.deltaTime / 2;
                    }
                    yield return null;
                }

                while (__instance.Fade.alpha >= 0f)
                {
                    __instance.Fade.alpha -= Time.deltaTime / 2;
                    yield return null;
                }
            }
            else
            {
                cutscene.PlayCutscene(2, player.transform.position, GameManager.GM.CameraManager.transform.rotation);
            }

            dying = false;
            yield break;
        }

        public static IEnumerator lookUpDeath(GTTOD_DeathObject __instance)
        {
            ac_CutsceneManager cutscene = GameManager.GM.gameObject.GetComponent<ac_CutsceneManager>();
            ac_CharacterController player = GameManager.GM.Player.GetComponent<ac_CharacterController>();
            GTTOD_Level level = GameObject.FindAnyObjectByType<GTTOD_Level>();
            GTTOD_HealthScript health = player.GetComponent<GTTOD_HealthScript>();
            InventoryScript inventory = player.GetComponent<InventoryScript>();

            while (__instance.Fade.alpha <= .99f)
            {
                if (__instance.Fade.alpha + Time.deltaTime * 6 > 1f)
                {
                    __instance.Fade.alpha = 1f;
                }
                else
                {
                    __instance.Fade.alpha += Time.deltaTime * 6;
                }
                yield return null;
            }

            Vector3 rotationEuler = GameManager.GM.CameraManager.transform.rotation.eulerAngles;
            rotationEuler.x = -90;
            Quaternion rotation = Quaternion.Euler(rotationEuler);
            cutscene.PlayCutscene(2, player.transform.position, rotation);
            yield return new WaitForEndOfFrame();
            AudioSource audioSource = GameObject.Find("Die(Clone)").GetComponent<AudioSource>();
            Animator animator = GameObject.Find("Die(Clone)").GetComponent<Animator>();
            __instance.StartCoroutine(falling(__instance));
            audioSource.volume = 0;
            animator.speed = 27;
            yield return new WaitForSeconds(.3f);
            animator.speed = .3f;
            while (__instance.Fade.alpha >= .8f)
            {
                __instance.Fade.alpha -= Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(3f);
            while (__instance.Fade.alpha <= .99f)
            {
                if (__instance.Fade.alpha + Time.deltaTime > 1f)
                {
                    __instance.Fade.alpha = 1f;
                }
                else
                {
                    __instance.Fade.alpha += Time.deltaTime;
                }
                yield return null;
            }
            animator.speed = 100;
            dying = false;
            yield break;
        }

        public static IEnumerator falling(GTTOD_DeathObject __instance)
        {
            GameObject Die = SceneManager.GetActiveScene().GetRootGameObjects()[0];
            GameObject waystation = GameManager.GM.gameObject.GetComponent<GTTOD_Manager>().Waystation.gameObject;

            float fallSpeed = 100;
            if (swimDeath) fallSpeed = 10;

            while (dying)
            {
                Vector3 targetPosition = Die.transform.position;
                targetPosition.y += fallSpeed;

                float step = fallSpeed * Time.deltaTime;
                Die.transform.position = Vector3.MoveTowards(Die.transform.position, targetPosition, step);


                targetPosition = waystation.transform.position;
                targetPosition.y += fallSpeed;

                waystation.transform.position = Vector3.MoveTowards(waystation.transform.position, targetPosition, step);

                yield return null;
            }

            swimDeath = false;
            yield break;
        }
    }
    [HarmonyPatch(typeof(GTTOD_DeathObject), "Update")]
    public class GTTOD_DeathObject_Update_Patch
    {
        public static bool Prefix(GTTOD_DeathObject __instance)
        {
            return GTTOD_DeathObject_Start_Patch.runUpdate;
        }
    }

    [HarmonyPatch(typeof(GTTOD_Level), "ResetPlayer")]
    partial class GTTOD_Level_ResetPlayer_Patch
    {
        public static bool Prefix(GTTOD_Level __instance)
        {
            if (GTTOD_DeathObject_Start_Patch.dying) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(GTTOD_Reflection), "ResetPlayer")]
    partial class GTTOD_Level_ResetPlayer_Patch
    {
        public static bool Prefix(GTTOD_Reflection __instance)
        {
            if (GTTOD_DeathObject_Start_Patch.dying) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(GTTOD_Dungeon), "ResetPlayer")]
    partial class GTTOD_Level_ResetPlayer_Patch
    {
        public static bool Prefix(GTTOD_Dungeon __instance)
        {
            if (GTTOD_DeathObject_Start_Patch.dying) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(GTTOD_HUB), "ResetPlayer")]
    partial class GTTOD_Level_ResetPlayer_Patch
    {
        public static bool Prefix(GTTOD_HUB __instance)
        {
            if (GTTOD_DeathObject_Start_Patch.dying) return false;
            return true;
        }
    }
}