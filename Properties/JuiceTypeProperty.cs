using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace JuicesMod.Properties
{
    public struct JuiceTypeProperty : INetworkSerializable
    {
        [ES3NonSerializable]
        public static Dictionary<string, JuiceTypeProperty> JUICE_TYPES = [];
        public static void Initialize()
        {
            JUICE_TYPES.Add("Carton",
                new JuiceTypeProperty("Carton",     1,  1.2f,   20, 40,     Config.Instance.cartonsRarity.Value)
            );
            JUICE_TYPES.Add("Premium",
                new JuiceTypeProperty("Premium",    2,  1.5f,   30, 60,     Config.Instance.premiumsRarity.Value)
            );
            JUICE_TYPES.Add("Pack",
                new JuiceTypeProperty("Pack",       3,  1.3f,   80, 140,    Config.Instance.packRarity.Value)
            );
        }

        public string Name;
        public int Index;
        public float Multiplier;
        public int MinValue;
        public int MaxValue;
        public int Rarity;

        public JuiceTypeProperty(string name, int index, float multiplier, int minValue, int maxValue, int rarity) {
            Name = name;
            Index = index;
            Multiplier = multiplier;
            MinValue = (int)Mathf.Ceil(minValue / 0.4f);
            MaxValue = (int)Mathf.Ceil(maxValue / 0.4f);
            Rarity = rarity;
        }
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref Index);
            serializer.SerializeValue(ref Multiplier);
            serializer.SerializeValue(ref MinValue);
            serializer.SerializeValue(ref MaxValue);
            serializer.SerializeValue(ref Rarity);
        }
    }
}
