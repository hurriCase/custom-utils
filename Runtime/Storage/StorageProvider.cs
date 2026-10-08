using System;
using CustomUtils.Runtime.Storage.Base;
using CustomUtils.Runtime.Storage.Persistent;
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
        /// Sets the storage provider to use for all persistent storage operations and clears
        /// <see cref="Local"/> and <see cref="Cloud"/>.
        /// Must be called before creating any <see cref="PersistentReactiveProperty{T}"/>,
        /// <see cref="PersistentObservableList{TValue}"/> or <see cref="PersistentObservableDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <param name="provider">The default provider, e.g. a <see cref="Providers.HybridStorageProvider"/></param>
        /// <returns>A builder to optionally expose the local and cloud providers</returns>
        public static Builder SetProvider([NotNull] IStorageProvider provider)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Local = null;
            Cloud = null;
            return default;
        }

        /// <summary>
        /// Exposes the optional local and cloud providers after <see cref="SetProvider"/>.
        /// </summary>
        [PublicAPI]
        public readonly struct Builder
        {
            /// <summary>
            /// Sets <see cref="Local"/>.
            /// </summary>
            /// <param name="local">Device-only storage provider</param>
            /// <returns>The same builder for chaining</returns>
            public Builder WithLocal([NotNull] IStorageProvider local)
            {
                Local = local ?? throw new ArgumentNullException(nameof(local));
                return this;
            }

            /// <summary>
            /// Sets <see cref="Cloud"/>.
            /// </summary>
            /// <param name="cloud">Remote storage provider</param>
            /// <returns>The same builder for chaining</returns>
            public Builder WithCloud([NotNull] ICloudStorageProvider cloud)
            {
                Cloud = cloud ?? throw new ArgumentNullException(nameof(cloud));
                return this;
            }
        }
    }
}
