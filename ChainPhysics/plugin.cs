using BepInEx;

namespace ChainPhysics
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class ChainPhysics : BaseUnityPlugin
    {
        void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded!");
        }
    }
}
