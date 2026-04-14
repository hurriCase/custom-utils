using JetBrains.Annotations;

namespace CustomUtils.Runtime.Serializer
{
    [PublicAPI]
    public interface IBytesSerializer
    {
        byte[] SerializeToBytes<T>(T data);
        T DeserializeFromBytes<T>(byte[] data);
        void DeserializeFromBytes<T>(byte[] data, ref T result) => result = DeserializeFromBytes<T>(data);
    }
}