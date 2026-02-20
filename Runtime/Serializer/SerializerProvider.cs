namespace CustomUtils.Runtime.Serializer
{
    internal static class SerializerProvider
    {
        static SerializerProvider()
        {
#if MEMORY_PACK_INSTALLED
            Serializer = new MemoryPackSerializer();
#elif NEWTONSOFT_INSTALLED
            Serializer = new NewtonsoftSerializer();
#endif
        }

        internal static ISerializer Serializer { get; }
    }
}