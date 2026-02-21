using JetBrains.Annotations;

namespace CustomUtils.Runtime.Serializer
{
    [PublicAPI]
    public static class SerializerProvider
    {
        static SerializerProvider()
        {
#if MEMORY_PACK_INSTALLED
            Serializer = new MemoryPackSerializer();
#elif NEWTONSOFT_INSTALLED
            Serializer = new NewtonsoftSerializer();
#else
            Serializer = new SystemTextJsonSerializer();
#endif
        }

        public static ISerializer Serializer { get; }
    }
}