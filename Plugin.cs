using BepInEx;
using HarmonyLib;
using LethalLib.Modules;
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

        private void Awake()
        {
            instance = this;

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "juicesmod");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);

            JuicesBuilder juicesBuilder = new JuicesBuilder(bundle);

            juicesBuilder.registerBrick("OrangeJuice001Item");
            juicesBuilder.registerBrick("AppleJuice001Item");
            juicesBuilder.registerBrick("PineappleJuice001Item");
            juicesBuilder.registerPremium("OrangeJuice002Item");
            juicesBuilder.registerPremium("AppleJuice002Item");
            juicesBuilder.registerPremium("PineappleJuice002Item");

            //TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            //node.clearPreviousText = true;
            //node.displayText = "\"Le jus d'orange ça pique !\"\nDaiick - 2020\n\n";
            //Items.RegisterShopItem(orangeJuice001, null, null, node, 0);

            Logger.LogInfo("Juices Mod");
        }
    }
}
