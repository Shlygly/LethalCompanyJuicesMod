﻿using JuicesMod.Properties;
using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JuicesMod
{
    public class JuicesBuilder
    {
        private readonly AssetBundle bundle;
        private Dictionary<Item, Tuple<FruitProperty, JuiceTypeProperty>> juices = [];
        private Dictionary<JuiceTypeProperty, Item> multifruits = [];

        public Item[] Juices
        {
            get {
                return [.. juices.Keys];
            }
        }

        public JuicesBuilder(AssetBundle bundle)
        {
            this.bundle = bundle;
        }

        public void registerJuice(FruitProperty fruit, JuiceTypeProperty type, Levels.LevelTypes levelType = Levels.LevelTypes.All)
        {
            try
            {
                Item juice = bundle.LoadAsset<Item>($"Assets/JuicesMod/Juices/{type.Name}s/{fruit.Name}Juice{type.Index:000}Item.asset");
                juice.minValue = type.MinValue;
                juice.maxValue = type.MaxValue;
                NetworkPrefabs.RegisterNetworkPrefab(juice.spawnPrefab);
                Utilities.FixMixerGroups(juice.spawnPrefab);
                Items.RegisterScrap(juice, type.Rarity, levelType);
                juices.Add(juice, new Tuple<FruitProperty, JuiceTypeProperty>(fruit, type));
                Plugin.Logger.LogInfo($"Registered juice : {fruit.Name} juice in {type.Name} format");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Can't register {fruit.Name} juice in {type.Name} format !");
            }
        }

        public void registerMultifruitJuice(JuiceTypeProperty type)
        {
            try
            { 
                Item juice = bundle.LoadAsset<Item>($"Assets/JuicesMod/Juices/{type.Name}s/MultifruitJuice{type.Index:000}Item.asset");
                NetworkPrefabs.RegisterNetworkPrefab(juice.spawnPrefab);
                Utilities.FixMixerGroups(juice.spawnPrefab);
                Items.RegisterScrap(juice, 0, Levels.LevelTypes.None);
                multifruits.Add(type, juice);
                Plugin.Logger.LogInfo($"Registered juice : Multifruit juice in {type.Name} format");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Can't register Multifruit juice in {type.Name} format !");
            }
}

        public bool hasJuiceProperty(GrabbableObject item)
        {
            return juices.ContainsKey(item.itemProperties);
        }

        public JuiceProperty getJuiceProperty(GrabbableObject item)
        {
            if (hasJuiceProperty(item))
            {
                Tuple<FruitProperty, JuiceTypeProperty> properties = juices[item.itemProperties];
                return new JuiceProperty(properties.Item1, properties.Item2, item.scrapValue);
            }
            throw new ArgumentException($"Unable to find juice property for item \"{item.name}\". Did you miss a check with \"hasJuiceProperty\" ?");
        }

        public bool isMultifruit(GrabbableObject item)
        {
            return multifruits.ContainsValue(item.itemProperties);
        }

        public Item getMultifruit(JuiceTypeProperty type)
        {
            if (multifruits.ContainsKey(type))
            {
                return multifruits[type];
            }
            return multifruits.Values.FirstOrDefault();
        }
    }
}
