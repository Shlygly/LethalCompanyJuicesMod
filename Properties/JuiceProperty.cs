using Unity.Netcode;

namespace JuicesMod.Properties
{
    public struct JuiceProperty : INetworkSerializable
    {
        public FruitProperty Fruit;
        public JuiceTypeProperty Type;
        public int ScrapValue;

        public JuiceProperty(FruitProperty fruit, JuiceTypeProperty type, int scrapValue)
        {
            Fruit = fruit;
            Type = type;
            ScrapValue = scrapValue;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            Fruit.NetworkSerialize(serializer);
            Type.NetworkSerialize(serializer);
            serializer.SerializeValue(ref ScrapValue);
        }
    }
}
