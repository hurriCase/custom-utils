using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable MemberCanBeInternal
namespace CustomUtils.Runtime.AssetLoader
{
    /// <summary>
    /// Generic utility class for loading Unity resources.
    /// Simple, clean implementation without caching (Unity handles caching internally).
    /// </summary>
    /// <typeparam name="TResource">The type of resource to load, must inherit from UnityEngine.Object.</typeparam>
    [PublicAPI]
    public static class ResourceLoader<TResource> where TResource : Object
    {
        /// <summary>
        /// Loads a resource of type TResource from the specified path within Resources folders.
        /// If no path is provided, it will attempt to determine the path from ResourceAttribute.
        /// </summary>
        /// <param name="resourcePath">
        /// The path WITHOUT file extension, relative to any Resources folder in the project.
        /// Examples: "UI/Icons/settings", "Audio/music_track", "Prefabs/Player", "Materials/stone".
        /// </param>
        /// <returns>The loaded resource or null if the resource could not be found.</returns>
        public static TResource Load(string resourcePath = null) =>
            PathUtility.TryGetResourcePath<TResource>(ref resourcePath)
                ? Resources.Load<TResource>(resourcePath)
                : null;

        /// <summary>
        /// Attempts to load a resource and returns whether the operation was successful.
        /// </summary>
        /// <param name="resource">When this method returns, contains the loaded resource if found; otherwise, the default value for the type.</param>
        /// <param name="path">
        /// The path WITHOUT file extension, relative to any Resources folder in the project.
        /// Examples: "UI/Icons/settings", "Audio/music_track", "Prefabs/Player", "Materials/stone".
        /// If null, path is determined from ResourceAttribute on TResource type.
        /// </param>
        /// <returns>True if the resource was successfully loaded; otherwise, false.</returns>
        public static bool TryLoad(out TResource resource, string path = null)
        {
            resource = Load(path);
            return resource;
        }

        /// <summary>
        /// Asynchronously loads a resource of type TResource from the specified path within Resources folders.
        /// If no path is provided, it will attempt to determine the path from ResourceAttribute.
        /// </summary>
        /// <param name="resourcePath">
        /// The path WITHOUT file extension, relative to any Resources folder in the project.
        /// Examples: "UI/Icons/settings", "Audio/music_track", "Prefabs/Player", "Materials/stone".
        /// </param>
        /// <param name="cancellationToken">Optional cancellation token to stop the loading operation.</param>
        /// <returns>A UniTask that represents the asynchronous load operation. The result is the loaded resource or null if not found.</returns>
        public static async UniTask<TResource> LoadAsync(string resourcePath = null,
            CancellationToken cancellationToken = default)
        {
            if (!PathUtility.TryGetResourcePath<TResource>(ref resourcePath))
                return null;

            var resourceRequest = Resources.LoadAsync<TResource>(resourcePath);

            await resourceRequest.ToUniTask(cancellationToken: cancellationToken);

            var resource = resourceRequest.asset as TResource;
            if (!resource)
                Debug.LogWarning($"[ResourceLoader::LoadAsync] Failed to load resource at path: {resourcePath}");

            return resource;
        }

        /// <summary>
        /// Loads all resources of type TResource from the specified directory within Resources folders.
        /// </summary>
        /// <param name="path">
        /// The directory path WITHOUT trailing slash, relative to any Resources folder.
        /// Examples: "UI/Icons", "Audio/Effects", "Prefabs/Enemies", "Materials".
        /// </param>
        /// <returns>An array of loaded resources or null if no resources were found.</returns>
        public static TResource[] LoadAll(string path)
        {
            var resources = Resources.LoadAll<TResource>(path);

            if (resources != null && resources.Length != 0)
                return resources;

            Debug.LogWarning($"[ResourceLoader::LoadAll] No resources found at path: {path}");
            return null;
        }

        /// <summary>
        /// Attempts to load all resources of type TResource from the specified directory.
        /// </summary>
        /// <param name="path">
        /// The directory path WITHOUT trailing slash, relative to any Resources folder.
        /// Examples: "UI/Icons", "Audio/Effects", "Prefabs/Enemies", "Materials".
        /// </param>
        /// <param name="resources">When this method returns, contains the loaded resources if found; otherwise, null.</param>
        /// <returns>True if resources were successfully loaded; otherwise, false.</returns>
        public static bool TryLoadAll(string path, out TResource[] resources)
            => (resources = LoadAll(path)) != null;
    }
}