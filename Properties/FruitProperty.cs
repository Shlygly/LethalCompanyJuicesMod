using Unity.Netcode;
using UnityEngine;

namespace JuicesMod.Properties
{
    public struct FruitProperty : INetworkSerializable
    {
        public static FruitProperty[] FRUITS = [
            new FruitProperty("Orange", new Color(1.0f, 0.5882352941176471f, 0.0f)),
            new FruitProperty("Apple", new Color(0.7803921568627451f, 0.5568627450980392f, 0.12941176470588237f)), 
            new FruitProperty("Pineapple", new Color(1.0f, 0.8784313725490196f, 0.5137254901960784f)), 
            new FruitProperty("Tomato", new Color(0.9215686274509803f, 0.0f, 0.0f)), 
            new FruitProperty("Prune", new Color(0.44313725490196076f, 0.0f, 0.5686274509803921f))
        ];

        public string Name;
        public Color Color;

        public FruitProperty(string name, Color color)
        {
            Name = name;
            Color = color;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref Color);
        }
    }
}
