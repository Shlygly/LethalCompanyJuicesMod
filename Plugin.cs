using BepInEx;
using BepInEx.Logging;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace JuicesMod
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class Plugin : BaseUnityPlugin
    {

        public static Plugin instance;
        internal new static ManualLogSource Logger;
        internal static Config BoundConfig { get; private set; } = null;

        private string[] FRUITS = ["Orange", "Apple", "Pineapple", "Tomato", "Prune"];

        private void Awake()
        {
            instance = this;
            Logger = base.Logger;
            BoundConfig = new Config(base.Config);

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "juicesmod");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);

            JuicesBuilder juicesBuilder = new JuicesBuilder(bundle);

            foreach(string fruit in FRUITS)
            {
                juicesBuilder.registerCarton($"{fruit}Juice001Item");
                juicesBuilder.registerPremium($"{fruit}Juice002Item");
            }

            //TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            //node.clearPreviousText = true;
            //node.displayText = "\"Le jus d'orange ça pique !\"\nDaiick - 2020\n\n";
            //Items.RegisterShopItem(orangeJuice001, null, null, node, 0);

            Logger.LogInfo("Juices Mod");
        }
    }
}
