using LethalLib.Extras;
using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace JuicesMod
{
    public class UnlockablesBuilder
    {
        private readonly AssetBundle bundle;

        public UnlockablesBuilder(AssetBundle bundle)
        {
            this.bundle = bundle;
        }

        public void register(string assetName, int price, StoreType storeType)
        {
            try
            {
                UnlockableItemDef itemDef = bundle.LoadAsset<UnlockableItemDef>($"Assets/JuicesMod/Unlockables/{assetName}/UnlockableItemDef.asset");

                NetworkPrefabs.RegisterNetworkPrefab(itemDef.unlockable.prefabObject);
                Utilities.FixMixerGroups(itemDef.unlockable.prefabObject);

                Unlockables.RegisterUnlockable(itemDef, price, storeType);

                Plugin.Logger.LogInfo($"Registered unlockable : {assetName}");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Can't register {assetName} !");
            }
        }
    }
}
