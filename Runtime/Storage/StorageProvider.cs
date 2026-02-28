using CustomUtils.Runtime.Storage.Base;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Storage
{
    /// <summary>
    /// Global access point for the active <see cref="IStorageProvider"/>.
    /// Call <see cref="SetProvider"/> once at startup before any storage operations.
    /// </summary>
    [PublicAPI]
    public static class StorageProvider
    {
        /// <summary>
        /// The active storage provider. Throws if <see cref="SetProvider"/> has not been called.
        /// </summary>
        public static IStorageProvider Provider { get; private set; }

        /// <summary>
        /// Sets the storage provider to use for all persistent storage operations.
        /// Must be called before any <see cref="PersistentReactiveProperty{T}"/> or
        /// <see cref="PersistentObservableDictionary{TKey,TValue}"/> initialization.
        /// </summary>
        public static void SetProvider(IStorageProvider provider)
        {
            Provider = provider;
        }
    }
}