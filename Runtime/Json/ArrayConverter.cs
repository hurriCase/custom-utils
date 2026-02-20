using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CustomUtils.Runtime.Json
{
    /// <inheritdoc />
    /// <summary>
    /// JSON converter for array wrapper types that implement
    /// <see cref="T:CustomUtils.Runtime.Json.IWrapper`1">IWrapper{T}</see>.
    /// </summary>
    /// <typeparam name="TWrapper">The wrapper type that implements
    /// <see cref="T:CustomUtils.Runtime.Json.IWrapper`1">IWrapper{TData}</see>.</typeparam>
    /// <typeparam name="TData">The type of data stored in the array.</typeparam>
    [PublicAPI]
    public sealed class ArrayConverter<TWrapper, TData> : JsonConverter<TWrapper>
        where TWrapper : IWrapper<TData>, new()
    {
        public override void WriteJson(JsonWriter writer, TWrapper value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Items);
        }

        public override TWrapper ReadJson(
            JsonReader reader,
            Type objectType,
            TWrapper existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var array = serializer.Deserialize<TData[]>(reader);
            var wrapper = new TWrapper();
            wrapper.SetItems(array);
            return wrapper;
        }
    }
}