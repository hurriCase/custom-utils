using System.Text;
using UnityEngine;

namespace CustomUtils.Runtime.Serializer
{
    internal sealed class SystemTextJsonSerializer : ISerializer
    {
        public byte[] Serialize<T>(T data) => Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
        public T Deserialize<T>(byte[] data) => JsonUtility.FromJson<T>(Encoding.UTF8.GetString(data));
    }
}