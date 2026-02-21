using JetBrains.Annotations;

namespace CustomUtils.Runtime.Serializer
{
    [PublicAPI]
    public interface ISerializer
    {
        byte[] Serialize<T>(T data);
        T Deserialize<T>(byte[] data);
        void Deserialize<T>(byte[] data, ref T result) => result = Deserialize<T>(data);
    }
}