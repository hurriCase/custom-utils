using JetBrains.Annotations;
using UnityEngine.AddressableAssets;

#if MEMORY_PACK_INSTALLED
using MemoryPack;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomUtils.Runtime.AddressableSystem
{
    /// <summary>
    /// Lightweight struct for caching sprite asset references by GUID.
    /// </summary>
    [PublicAPI]
#if MEMORY_PACK_INSTALLED
    [MemoryPackable]
#endif

    // ReSharper disable once PartialTypeWithSinglePart | required by MemoryPack
    public readonly partial struct CachedSprite
    {
        /// <summary>
        /// Gets the Addressable asset GUID.
        /// </summary>
        public string AssetGUID { get; }

        /// <summary>
        /// Initializes a new CachedSprite with the specified GUID.
        /// </summary>
        /// <param name="assetGUID">The Addressable asset GUID.</param>
#if MEMORY_PACK_INSTALLED
        [MemoryPackConstructor]
#endif
        public CachedSprite(string assetGUID) => AssetGUID = assetGUID;

        /// <summary>
        /// Initializes a new CachedSprite from an AssetReference.
        /// </summary>
        /// <param name="assetReference">The asset reference to cache.</param>
        public CachedSprite(AssetReference assetReference) => AssetGUID = assetReference.AssetGUID;

        /// <summary>
        /// Creates a CachedSprite from an asset path by resolving its GUID.
        /// </summary>
        /// <param name="assetPath">The asset path (e.g., "Assets/Sprites/icon.png").</param>
        public static CachedSprite FromPath(string assetPath)
        {
#if UNITY_EDITOR
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            return new CachedSprite(guid);
#else
            return default;
#endif
        }

        /// <summary>
        /// Gets whether the cached sprite has a valid GUID.
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(AssetGUID);
    }
}