namespace CustomUtils.Runtime.Serializer
{
    internal interface ISerializer
    {
        byte[] Serialize<T>(T data);
        T Deserialize<T>(byte[] data);
        byte[] SerializeArray<T>(T[] objects);
        void DeserializeArray<T>(byte[] data, ref T[] result);
    }
}