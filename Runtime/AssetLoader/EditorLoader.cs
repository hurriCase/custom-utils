// Put in the runtime assembly due to SingletonScriptableObject

#if UNITY_EDITOR
using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using Object = UnityEngine.Object;

namespace CustomUtils.Runtime.AssetLoader
{
    /// <summary>
    /// Generic utility class for loading Unity resources from multiple sources.
    /// Attempts to load resources from Editor Default Resources, standard Resources folders,
    /// and falls back to AssetDatabase searches when needed.
    /// </summary>
    /// <typeparam name="TResource">The type of resource to load, must inherit from UnityEngine.Object.</typeparam>
    [PublicAPI]
    public static class EditorLoader<TResource> where TResource : Object
    {
        /// <summary>
        /// Loads a resource of type TResource using multiple loading strategies.
        /// First tries EditorGUIUtility.Load for Editor Default Resources,
        /// then tries Resources.Load for standard Resources folders,
        /// then tries AssetDatabase.LoadAssetAtPath with the provided full path,
        /// and finally searches the entire project for assets of the specified type.
        /// </summary>
        /// <param name="resourcePath">
        /// The resource path WITH file extension, relative to Editor Default Resources folder.
        /// Examples: "UI/Icons/settings.png", "Audio/music_track.wav", "Materials/stone.mat".
        /// If null, path is determined from ResourceAttribute on TResource type.
        /// </param>
        /// <param name="fullPath">
        /// The complete asset path WITH file extension, relative to the project root.
        /// Examples: "Assets/UI/Icons/settings.png", "Assets/Prefabs/Player.prefab".
        /// </param>
        /// <returns>The loaded resource or null if the resource could not be found.</returns>
        public static TResource Load(string resourcePath = null, string fullPath = null)
        {
            if (PathUtility.TryGetResourcePath<TResource>(ref resourcePath) is false)
                return null;

            var resource = EditorGUIUtility.Load(resourcePath) as TResource;

            if (resource ||
                ResourceLoader<TResource>.TryLoad(out resource, Path.GetFileNameWithoutExtension(resourcePath)))
                return resource;

            if (string.IsNullOrEmpty(fullPath) is false)
            {
                resource = AssetDatabase.LoadAssetAtPath<TResource>(fullPath);

                if (resource)
                    return resource;
            }

            var guids = AssetDatabase.FindAssets($"t:{nameof(TResource)}");
            if (guids.Length <= 0)
                return null;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<TResource>(path);
        }

        /// <summary>
        /// Attempts to load a resource using the same multi-strategy approach as Load().
        /// </summary>
        /// <param name="resource">When this method returns, contains the loaded resource if found; otherwise, the default value for the type.</param>
        /// <param name="resourcePath">
        /// The resource path WITH file extension, relative to Editor Default Resources folder.
        /// Examples: "UI/Icons/settings.png", "Audio/music_track.wav", "Materials/stone.mat".
        /// If null, path is determined from ResourceAttribute on TResource type.
        /// </param>
        /// <param name="fullPath">
        /// The complete asset path WITH file extension, relative to the project root.
        /// Examples: "Assets/UI/Icons/settings.png", "Assets/Prefabs/Player.prefab".
        /// </param>
        /// <returns>True if the resource was successfully loaded; otherwise, false.</returns>
        public static bool TryLoad(out TResource resource, string resourcePath = null, string fullPath = null)
        {
            resource = Load(resourcePath, fullPath);
            return resource;
        }
    }
}
#endif