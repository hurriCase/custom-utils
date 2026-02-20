#if MEMORY_PACK_INSTALLED
namespace CustomUtils.Runtime.Serializer
{
    internal sealed class MemoryPackSerializer : ISerializer
    {
        public byte[] Serialize<T>(T data) => MemoryPack.MemoryPackSerializer.Serialize(data);
        public T Deserialize<T>(byte[] data) => MemoryPack.MemoryPackSerializer.Deserialize<T>(data);
        public byte[] SerializeArray<T>(T[] objects) => MemoryPack.MemoryPackSerializer.Serialize(objects);

        public void DeserializeArray<T>(byte[] data, ref T[] result) =>
            MemoryPack.MemoryPackSerializer.Deserialize(data, ref result);
    }
}
#endif