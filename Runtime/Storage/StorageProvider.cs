using System;
using CustomUtils.Runtime.Storage.Base;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Storage
{
    /// <summary>
    /// Global access point for the storage providers.
    /// Call <see cref="SetProvider"/> once at startup before any storage operations.
    /// </summary>
    [PublicAPI]
    public static class StorageProvider
    {
        /// <summary>
        /// The active storage provider, used by default for all persistent storage.
        /// </summary>
        public static IStorageProvider Provider { get; private set; }

        /// <summary>
        /// Device-only storage provider, or null if none was set via <see cref="Builder.WithLocal"/>.
        /// </summary>
        public static IStorageProvider Local { get; private set; }

        /// <summary>
        /// Remote storage provider, or null if none was set via <see cref="Builder.WithCloud"/>.
        /// </summary>
        public static ICloudStorageProvider Cloud { get; private set; }

        /// <summary>
        /// Sets the storage provider to use for all persistent storage operations.
        /// Must be called before any <see cref="PersistentReactiveProperty{T}"/> or
        /// <see cref="PersistentObservableDictionary{TKey,TValue}"/> initialization.
        /// </summary>
        /// <returns>A builder to optionally expose the local and cloud providers.</returns>
        public static Builder SetProvider(IStorageProvider provider)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Local = null;
            Cloud = null;
            return default;
        }

        [PublicAPI]
        public readonly struct Builder
        {
            public Builder WithLocal(IStorageProvider local)
            {
                Local = local ?? throw new ArgumentNullException(nameof(local));
                return this;
            }

            public Builder WithCloud(ICloudStorageProvider cloud)
            {
                Cloud = cloud ?? throw new ArgumentNullException(nameof(cloud));
                return this;
            }
        }
    }
}
