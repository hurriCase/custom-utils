using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Serializer
{
    [PublicAPI]
    public sealed class SystemTextJsonSerializer : IBytesSerializer, IStringSerializer
    {
        public byte[] SerializeToBytes<T>(T data) => Encoding.UTF8.GetBytes(SerializeToString(data));
        public T DeserializeFromBytes<T>(byte[] data) => DeserializeFromString<T>(Encoding.UTF8.GetString(data));
        public string SerializeToString<T>(T data) => JsonUtility.ToJson(data);
        public T DeserializeFromString<T>(string data) => JsonUtility.FromJson<T>(data);
    }
}