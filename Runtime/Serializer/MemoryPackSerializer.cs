using System;
using JetBrains.Annotations;

#if MEMORY_PACK_INSTALLED
namespace CustomUtils.Runtime.Serializer
{
    [PublicAPI]
    public sealed class MemoryPackSerializer : IBytesSerializer, IStringSerializer
    {
        public byte[] SerializeToBytes<T>(T data) => MemoryPack.MemoryPackSerializer.Serialize(data);
        public string SerializeToString<T>(T data) => Convert.ToBase64String(SerializeToBytes(data));
        public T DeserializeFromBytes<T>(byte[] data) => MemoryPack.MemoryPackSerializer.Deserialize<T>(data);

        public void DeserializeFromBytes<T>(byte[] data, ref T result)
            => MemoryPack.MemoryPackSerializer.Deserialize(data, ref result);

        public T DeserializeFromString<T>(string data) => DeserializeFromBytes<T>(Convert.FromBase64String(data));
    }
}
#endif