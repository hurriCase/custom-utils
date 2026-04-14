using JetBrains.Annotations;

namespace CustomUtils.Runtime.Serializer
{
    [PublicAPI]
    public interface IStringSerializer
    {
        string SerializeToString<T>(T data);
        T DeserializeFromString<T>(string data);
    }
}