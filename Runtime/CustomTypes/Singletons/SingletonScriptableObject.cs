using System.Reflection;
using CustomUtils.Runtime.AssetLoader;
using JetBrains.Annotations;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace CustomUtils.Runtime.CustomTypes.Singletons
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for singleton ScriptableObjects that are automatically loaded or created when accessed.
    /// </summary>
    /// <typeparam name="T">The type of ScriptableObject to create as a singleton.</typeparam>
    [PublicAPI]
    public abstract class SingletonScriptableObject<T> : ScriptableObject
        where T : SingletonScriptableObject<T>
    {
        private static T _instance;

        /// <summary>
        /// Gets the singleton instance of the ScriptableObject.
        /// If the instance doesn't exist, it will be loaded from resources or created if necessary.
        /// The location is determined by the ResourceAttribute applied to the class.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance)
                    return _instance;

                return _instance = LoadOrCreate();
            }
        }

#if UNITY_EDITOR
        static SingletonScriptableObject()
        {
            SingletonResetter.RegisterResetAction(static () => _instance = null);
        }
#endif

        /// <summary>
        /// Loads an existing instance or creates a new one if not found.
        /// </summary>
        /// <returns>The loaded or created instance.</returns>
        private static T LoadOrCreate()
        {
            var type = typeof(T);
            var attribute = type.GetCustomAttribute<ResourceAttribute>();
            if (attribute == null)
            {
                Debug.LogError($"[SingletonScriptableObject::LoadOrCreate] {type.Name} missing ResourceAttribute");
                return null;
            }

            if (TryLoadExisting(attribute, out var resource))
                return resource;

#if UNITY_EDITOR
            Debug.Log($"[SingletonScriptableObject::LoadOrCreate] Could not load {type.Name}, creating new asset...");
            return CreateAndSaveAsset(attribute);
#else
            Debug.LogError($"[SingletonScriptableObject::LoadOrCreate] " +
                           $"Could not load {type.Name} and cannot create assets at runtime.");
            return null;
#endif
        }

        private static bool TryLoadExisting(ResourceAttribute attribute, out T resource)
        {
#if UNITY_EDITOR
            return attribute.IsEditorResource
                ? EditorLoader<T>.TryLoad(out resource)
                : ResourceLoader<T>.TryLoad(out resource);
#else
            if (attribute.IsEditorResource is false)
                return ResourceLoader<T>.TryLoad(out resource);

            Debug.LogWarning($"[SingletonScriptableObject] Cannot load editor resource {typeof(T).Name} at runtime.");

            resource = null;
            return false;
#endif
        }

#if UNITY_EDITOR

        private static T CreateAndSaveAsset(ResourceAttribute attribute)
        {
            var assetPath = GetAssetPath(attribute);

            var existingAsset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (existingAsset)
            {
                Debug.Log($"[SingletonScriptableObject::CreateAndSaveAsset] Asset already exists at path: {assetPath}");
                return existingAsset;
            }

            var directory = Path.GetDirectoryName(assetPath);
            if (string.IsNullOrEmpty(directory) is false && Directory.Exists(directory) is false)
                Directory.CreateDirectory(directory);

            var resource = CreateInstance<T>();

            Debug.Log($"[SingletonScriptableObject::CreateAndSaveAsset] Creating asset at path: {assetPath}");

            AssetDatabase.CreateAsset(resource, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return resource;
        }

        private static string GetAssetPath(ResourceAttribute attribute)
        {
            var resourceFolderName = attribute.IsEditorResource ? "Editor Default Resources" : "Resources";

            return string.IsNullOrEmpty(attribute.FullPath)
                ? $"Assets/{resourceFolderName}/{attribute.Name}.asset"
                : $"{attribute.FullPath}/{attribute.Name}.asset";
        }

#endif
    }
}