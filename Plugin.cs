using BepInEx;
using BepInEx.Logging;
using LethalLib.Extras;
using LethalLib.Modules;
using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using System.Collections;
using JuicesMod.Behaviours;
using GameNetcodeStuff;
using JuicesMod.Properties;
using System.Linq;

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

            foreach (JuiceTypeProperty type in JuiceTypeProperty.JUICE_TYPES.Values)
            {
                foreach (FruitProperty fruit in FruitProperty.FRUITS)
                {
                    JuicesBuilder.registerJuice(fruit, type);
                }
                JuicesBuilder.registerMultifruitJuice(type);
            }

            UnlockableItemDef juiceBlender = bundle.LoadAsset<UnlockableItemDef>("Assets/JuicesMod/ShipJuiceBlenderItem.asset");
            if (juiceBlender?.unlockable?.prefabObject != null)
            {
                NetworkPrefabs.RegisterNetworkPrefab(juiceBlender.unlockable.prefabObject);
                Utilities.FixMixerGroups(juiceBlender.unlockable.prefabObject);
            }
            if (juiceBlender != null)
            {
                Unlockables.RegisterUnlockable(juiceBlender, 80, StoreType.ShipUpgrade);
            }
            else
            {
                Logger.LogError("Unable to load JUICE BLENDER !!!");
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
