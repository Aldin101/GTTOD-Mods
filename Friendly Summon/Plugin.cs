using UnityEngine;
using BepInEx;
using K8Lib.Inventory;
using System.Reflection;
using System.IO;

namespace FriendlySummon
{
    [BepInDependency(K8Lib.PluginInfo.PLUGIN_GUID)]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class FriendlySummon : BaseUnityPlugin
    {
        Sprite icon;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("FriendlySummon.icon.sprite"))
            {
                if (stream == null)
                {
                    Logger.LogError("Failed to load asset bundle");
                    return;
                }

                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    icon = AssetBundle.LoadFromMemory(memoryStream.ToArray()).LoadAsset<Sprite>("icon.sprite");
                }
            }
        }

        private void Start()
        {
            new InventoryIcon("friendlySummon", "SUMMON FIRENDLY ENEMY", "SUMMON A GOOD BOY", null, icon, onGridClick);
        }

        private void onGridClick()
        {

            GTTOD_UpgradesManager upgrades = FindObjectOfType<GTTOD_UpgradesManager>();

            GTTOD_AIManager ai = FindAnyObjectByType<GTTOD_AIManager>();

            Transform closestSpawn = ai.GetClosestSpawn(ai.SpawnPoints);
            int num = Random.Range(0, upgrades.QueenOfWandsSummons.Count);
            if (closestSpawn != null)
            {
                GTTOD_HUD hud = FindObjectOfType<GTTOD_HUD>();

                hud.CenterPopUp("A " + upgrades.QueenOfWandsSummons[num].EnemyName + " HAS BEEN SUMMONED", 18, 2f);
                Object.Instantiate<GameObject>(upgrades.QueenOfWandsSummons[num].GenericSpawn, closestSpawn.position, closestSpawn.rotation);
            }
        }
    }
}