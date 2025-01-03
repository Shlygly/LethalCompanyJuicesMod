using BepInEx;
using BepInEx.Logging;
using LethalLib.Modules;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using JuicesMod.Properties;
using HarmonyLib;
using JuicesMod.Patches;

namespace JuicesMod
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class Plugin : BaseUnityPlugin
    {

        public static Plugin instance;
        internal new static ManualLogSource Logger;
        internal static Config BoundConfig { get; private set; } = null;

        public JuicesBuilder JuicesBuilder { get; private set; } = null;
        public UnlockablesBuilder UnlockablesBuilder { get; private set; } = null;
        public Item VitaminDetector { get; private set; } = null;

        private void Awake()
        {
            instance = this;
            Logger = base.Logger;
            BoundConfig = new Config(base.Config);

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "juicesmod");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);

            RegisterRPCs();
            JuiceTypeProperty.Initialize();

            JuicesBuilder = new JuicesBuilder(bundle);
            UnlockablesBuilder = new UnlockablesBuilder(bundle);

            foreach (JuiceTypeProperty type in JuiceTypeProperty.JUICE_TYPES.Values)
            {
                foreach (FruitProperty fruit in FruitProperty.FRUITS)
                {
                    JuicesBuilder.registerJuice(fruit, type);
                }
                JuicesBuilder.registerMultifruitJuice(type);
            }

            UnlockablesBuilder.register("JuiceBlender", 80, StoreType.ShipUpgrade);
            UnlockablesBuilder.register("Neons/Sapik", 24, StoreType.ShipUpgrade);
            UnlockablesBuilder.register("Neons/Cedhou", 24, StoreType.ShipUpgrade);
            UnlockablesBuilder.register("Neons/Saifrai", 24, StoreType.ShipUpgrade);

            try
            {
                VitaminDetector = bundle.LoadAsset<Item>($"Assets/JuicesMod/VitaminDetectorItem.asset");
                NetworkPrefabs.RegisterNetworkPrefab(VitaminDetector.spawnPrefab);
                Utilities.FixMixerGroups(VitaminDetector.spawnPrefab);

                TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
                node.clearPreviousText = true;
                node.displayText = "A radar that detects nearby fruit juices.\n\n";
                Items.RegisterShopItem(VitaminDetector, null, null, node, 74);

                Logger.LogInfo("Registered Vitamin Detector");
            }
            catch (Exception e)
            {
                Logger.LogError("Can't register Vitamin Detector !");
            }

            try
            {
                Harmony.CreateAndPatchAll(typeof(PlayerControllerBPatch));
            }
            catch (Exception e)
            {
                Logger.LogError("Can't apply Harmony patches !");
            }

            Logger.LogInfo("Juices Mod Loaded");
        }

        private void RegisterRPCs()
        {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type type in types)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (MethodInfo method in methods)
                {
                    object[] attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
}
