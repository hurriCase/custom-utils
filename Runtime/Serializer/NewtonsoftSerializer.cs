#if NEWTONSOFT_INSTALLED
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CustomUtils.Runtime.Serializer
{
    [PublicAPI]
    public sealed class NewtonsoftSerializer : IBytesSerializer, IStringSerializer
    {
        public byte[] SerializeToBytes<T>(T data) => Encoding.UTF8.GetBytes(SerializeToString(data));
        public string SerializeToString<T>(T data) => JsonConvert.SerializeObject(data);
        public T DeserializeFromBytes<T>(byte[] data) => DeserializeFromString<T>(Encoding.UTF8.GetString(data));
        public T DeserializeFromString<T>(string data) => JsonConvert.DeserializeObject<T>(data);
    }
}
#endif