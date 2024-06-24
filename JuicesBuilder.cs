using LethalLib.Modules;
using UnityEngine;

namespace JuicesMod
{
    internal class JuicesBuilder
    {
        private readonly AssetBundle bundle;

        public JuicesBuilder(AssetBundle bundle)
        {
            this.bundle = bundle;
        }

        private void registerJuice(string asset, int minValue, int maxValue, int rarity, Levels.LevelTypes levelType = Levels.LevelTypes.All)
        {
            Item juice = bundle.LoadAsset<Item>($"Assets/Juices/{asset}.asset");
            juice.minValue = minValue;
            juice.maxValue = maxValue;
            NetworkPrefabs.RegisterNetworkPrefab(juice.spawnPrefab);
            Utilities.FixMixerGroups(juice.spawnPrefab);
            Items.RegisterScrap(juice, rarity, levelType);
        }

        public void registerBrick(string asset)
        {
            registerJuice(
                asset,
                38, 75, // 15 - 30
                80
            );
        }

        public void registerPremium(string asset)
        {
            registerJuice(
                asset,
                75, 125, // 30 - 50
                60
            );
        }
    }
}
