using UnityEngine;
using BepInEx;
using K8Lib;
using System.Reflection;
using System.IO;

namespace FriendlySummon
{
    [BepInDependency(K8Lib.PluginInfo.PLUGIN_GUID)]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class FriendlySummon : BaseUnityPlugin
    {

        Sprite icon = null;
        AssetBundle bundle = null;

        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");
            loadAssetBundle();
        }


        private void Update()
        {
            addInventoryItem();
        }

        private void addInventoryItem()
        {
            if (bundle == null || icon == null)
            {
                loadAssetBundle();
                return;
            }
            InventoryManager.InventoryIcon inventoryItem = new InventoryManager.InventoryIcon("friendlySummon", "SUMMON FIRENDLY ENEMY", "SUMMON A GOOD BOY", -1, icon, onGridClick);
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

        private void loadAssetBundle()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream fileStream = assembly.GetManifestResourceStream("FriendlySummon.icon.sprite");

            if (fileStream == null)
            {
                Logger.LogError("Failed to load resource stream");
                return;
            }

            byte[] fieData = new byte[fileStream.Length];
            fileStream.Read(fieData, 0, (int)fileStream.Length);
            fileStream.Close();

            string tempPath = Path.Combine(Application.temporaryCachePath, "icon.sprite");
            File.WriteAllBytes(tempPath, fieData);

            bundle = AssetBundle.LoadFromFile(tempPath);

            if (bundle == null)
            {
                Logger.LogError("Failed to load asset bundle");
                return;
            }

            icon = bundle.LoadAsset<Sprite>("icon.sprite");

            if (icon == null)
            {
                Logger.LogError("Failed to load sprite");
                return;
            }

            File.Delete(tempPath);
        }
    }
}