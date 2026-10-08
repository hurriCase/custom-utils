using JetBrains.Annotations;

namespace CustomUtils.Runtime.Serializer
{
    [PublicAPI]
    public static class SerializerProvider
    {
        static SerializerProvider()
        {
#if MEMORY_PACK_INSTALLED
            BytesSerializer = new MemoryPackSerializer();
            StringSerializer = new MemoryPackSerializer();
#else
            BytesSerializer = new NewtonsoftSerializer();
            StringSerializer = new NewtonsoftSerializer();
#endif
        }

        public static IBytesSerializer BytesSerializer { get; }
        public static IStringSerializer StringSerializer { get; }
    }
}