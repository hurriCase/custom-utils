#if NEWTONSOFT_INSTALLED
using System.Text;
using Newtonsoft.Json;

namespace CustomUtils.Runtime.Serializer
{
    internal sealed class NewtonsoftSerializer : ISerializer
    {
        public byte[] Serialize<T>(T data) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
        public T Deserialize<T>(byte[] data) => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
    }
}
#endif