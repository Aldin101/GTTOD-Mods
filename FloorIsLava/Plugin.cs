using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using K8Lib;

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
            Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            enabled = Config.Bind("Settings", "Enabled", true, "");
            burning = Config.Bind("Settings", "Burning", true, "Sets your on fire when you touch the floor, overrides damage value");
            damage = Config.Bind("Settings", "Damage", 3, "How much damage you take when you touch the floor");
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

            addSettings();
        }

        private void addSettings()
        {
            SettingsManager.SettingsElement.TitleBar titleBar = new SettingsManager.SettingsElement.TitleBar("lava", "THE FLOOR IS LAVA");
            SettingsManager.SettingsElement.CheckBox enabledToggle = new SettingsManager.SettingsElement.CheckBox("lavaEnabled", "ENABLED", enabled.Value, delegate (bool value)
            {
                enabled.Value = value;
            });
            SettingsManager.SettingsElement.CheckBox burningToggle = new SettingsManager.SettingsElement.CheckBox("lavaBurning", "BURNING (overrides damage value below)", burning.Value, delegate (bool value)
            {
                burning.Value = value;
            });
            SettingsManager.SettingsElement.Slider damageSlider = new SettingsManager.SettingsElement.Slider("lavaDamage", "DAMAGE", damage.Value, 1, 25, true, delegate (float value)
            {
                damage.Value = (int)value;
            });
        }
    }
}