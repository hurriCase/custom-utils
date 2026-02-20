using System.Text;
using Newtonsoft.Json;

namespace CustomUtils.Runtime.Serializer
{
    public sealed class NewtonsoftSerializer : ISerializer
    {
        public byte[] Serialize<T>(T data) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
        public T Deserialize<T>(byte[] data) => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
        public byte[] SerializeArray<T>(T[] objects) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(objects));

        public void DeserializeArray<T>(byte[] data, ref T[] result) =>
            result = JsonConvert.DeserializeObject<T[]>(Encoding.UTF8.GetString(data));
    }
}