using JuicesMod.Properties;
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
            get
            {
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
                Item juice = UnityEngine.Object.Instantiate(bundle.LoadAsset<Item>($"Assets/JuicesMod/Juices/{type.Name}JuiceItem.asset"));
                juice.itemName = string.Format(type.Label, fruit.Name);
                juice.minValue = type.MinValue;
                juice.maxValue = type.MaxValue;
                juice.spawnPrefab = NetworkPrefabs.CloneNetworkPrefab(
                    bundle.LoadAsset<GameObject>($"Assets/JuicesMod/Juices/{type.Name}Juice.prefab"),
                    juice.itemName
                );

                PhysicsProp prop = juice.spawnPrefab.GetComponent<PhysicsProp>();
                prop.itemProperties = juice;

                Texture2D texture;
                Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
                if (type.Name == "Pack")
                {
                    texture = bundle.LoadAsset<Texture2D>($"Assets/JuicesMod/Sources/Textures/{fruit.Name}JuiceCarton.png");
                    textures.Add("Model/JuiceBox", texture);
                    textures.Add("Model/JuiceBox.001", texture);
                    textures.Add("Model/JuiceBox.002", texture);
                    textures.Add("Model/JuiceBox.003", texture);
                    textures.Add("Model/JuiceBox.004", texture);
                    textures.Add("Model/JuiceBox.005", texture);
                }
                else
                {
                    texture = bundle.LoadAsset<Texture2D>($"Assets/JuicesMod/Sources/Textures/{fruit.Name}Juice{type.Name}.png");
                    textures.Add("Model", texture);
                }
                foreach (KeyValuePair<string, Texture2D> go in textures)
                {
                    MeshRenderer renderer = juice.spawnPrefab.transform.Find(go.Key).GetComponent<MeshRenderer>();
                    Material[] materials = renderer.materials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        Material variant = new(materials[i])
                        {
                            mainTexture = go.Value
                        };
                        materials[i] = variant;
                    }
                    renderer.materials = materials;
                }

                ScanNodeProperties scanNode = juice.spawnPrefab.transform.Find("ScanNode").GetComponent<ScanNodeProperties>();
                scanNode.headerText = juice.itemName;

                NetworkPrefabs.RegisterNetworkPrefab(juice.spawnPrefab);
                Utilities.FixMixerGroups(juice.spawnPrefab);
                Items.RegisterScrap(juice, type.Rarity, levelType);
                juices.Add(juice, new Tuple<FruitProperty, JuiceTypeProperty>(fruit, type));
                Plugin.Logger.LogInfo($"Registered juice : {fruit.Name} juice in {type.Name} format");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Can't register {fruit.Name} juice in {type.Name} format !\n{ex}");
            }
        }

        public void registerMultifruitJuice(JuiceTypeProperty type)
        {
            try
            {
                Item juice = UnityEngine.Object.Instantiate(bundle.LoadAsset<Item>($"Assets/JuicesMod/Juices/{type.Name}JuiceItem.asset"));
                juice.itemName = string.Format(type.Label, "Multifruit");
                juice.spawnPrefab = NetworkPrefabs.CloneNetworkPrefab(
                    bundle.LoadAsset<GameObject>($"Assets/JuicesMod/Juices/{type.Name}Juice.prefab"),
                    juice.itemName
                );

                PhysicsProp prop = juice.spawnPrefab.GetComponent<PhysicsProp>();
                prop.itemProperties = juice;

                Texture2D texture;
                Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
                if (type.Name == "Pack")
                {
                    texture = bundle.LoadAsset<Texture2D>($"Assets/JuicesMod/Sources/Textures/MultifruitJuiceCarton.png");
                    textures.Add("Model/JuiceBox", texture);
                    textures.Add("Model/JuiceBox.001", texture);
                    textures.Add("Model/JuiceBox.002", texture);
                    textures.Add("Model/JuiceBox.003", texture);
                    textures.Add("Model/JuiceBox.004", texture);
                    textures.Add("Model/JuiceBox.005", texture);
                }
                else
                {
                    texture = bundle.LoadAsset<Texture2D>($"Assets/JuicesMod/Sources/Textures/MultifruitJuice{type.Name}.png");
                    textures.Add("Model", texture);
                }
                foreach (KeyValuePair<string, Texture2D> go in textures)
                {
                    MeshRenderer renderer = juice.spawnPrefab.transform.Find(go.Key).GetComponent<MeshRenderer>();
                    Material[] materials = renderer.materials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        Material variant = new(materials[i])
                        {
                            mainTexture = go.Value
                        };
                        materials[i] = variant;
                    }
                    renderer.materials = materials;
                }

                ScanNodeProperties scanNode = juice.spawnPrefab.transform.Find("ScanNode").GetComponent<ScanNodeProperties>();
                scanNode.headerText = juice.itemName;

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
