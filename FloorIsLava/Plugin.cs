using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib.Settings;

namespace FloorIsLava
{
    [BepInDependency(K8Lib.PluginInfo.PLUGIN_GUID)]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class FloorIsLava : BaseUnityPlugin
    {
        // Config
        private ConfigEntry<bool> enabled;
        private ConfigEntry<bool> burning;
        private ConfigEntry<int> damage;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            enabled = Config.Bind("Settings", "Enabled", true, "");
            burning = Config.Bind("Settings", "Burning", true, "Sets your on fire when you touch the floor, overrides damage value");
            damage = Config.Bind("Settings", "Damage", 3, "How much damage you take when you touch the floor");
        }

        private void Start()
        {
            new TitleBar("lava", "THE FLOOR IS LAVA");
            new CheckBox("lavaEnabled", "ENABLED", enabled.Value, delegate (bool value)
            {
                enabled.Value = value;
            });
            new CheckBox("lavaBurning", "BURNING (overrides damage value below)", burning.Value, delegate (bool value)
            {
                burning.Value = value;
            });
            new Slider("lavaDamage", "DAMAGE", 1, 25, damage.Value, true, delegate (float value)
            {
                damage.Value = (int)value;
            });
        }

        private void Update()
        {
            if (GameManager.GM == null) return;

            ac_CharacterController controller = GameManager.GM.Player.GetComponent<ac_CharacterController>();

            bool isGrounded = controller.CharacterGroundState == ac_CharacterController.GroundState.Grounded
                || controller.CharacterGroundState == ac_CharacterController.GroundState.SteadyGround
                || controller.CharacterGroundState == ac_CharacterController.GroundState.Sliding
                || controller.CharacterGroundState == ac_CharacterController.GroundState.Swimming;

            if (isGrounded && enabled.Value)
            {
                if (burning.Value)
                {
                    GTTOD_UpgradesManager upgrades = FindAnyObjectByType<GTTOD_UpgradesManager>();
                    upgrades.ToggleBurn(true);
                    AccessTools.Field(upgrades.GetType(), "BurnTimer").SetValue(upgrades, 4f);
                }
                else
                {
                    GTTOD_HealthScript heath = GameManager.GM.Player.GetComponent<GTTOD_HealthScript>();
                    heath.Damage(damage.Value);
                }
            }

        }
    }
}