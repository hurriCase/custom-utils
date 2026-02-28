using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Object"/>.
    /// </summary>
    [PublicAPI]
    public static class ObjectExtensions
    {
        /// <summary>
        /// Returns the object itself if it exists, null otherwise.
        /// </summary>
        /// <remarks>
        /// This method helps differentiate between a null reference and a destroyed Unity object. Unity's "== null" check
        /// can incorrectly return true for destroyed objects, leading to misleading behaviour. The OrNull method use
        /// Unity's "null check", and if the object has been marked for destruction, it ensures an actual null reference is returned,
        /// aiding in correctly chaining operations and preventing NullReferenceExceptions.
        /// </remarks>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="target">The object being checked.</param>
        /// <returns>The object itself if it exists and not destroyed, null otherwise.</returns>
        public static T AsNullable<T>(this T target) where T : Object => target ? target : null;

        /// <summary>
        /// Marks the specified Unity object as dirty in the editor, ensuring that any changes are saved during serialization.
        /// </summary>
        /// <typeparam name="T">The type of the object, inheriting from <see cref="Object"/>.</typeparam>
        /// <param name="target">The object to be marked as dirty.</param>
        /// <remarks>
        /// This method is only applicable in the Unity Editor
        /// and does nothing during play mode or when the object is part of a prefab asset.
        /// It ensures that modifications made to the object are registered by Unity's serialization system.
        /// </remarks>
        [Conditional("UNITY_EDITOR")]
        public static void MarkAsDirty<T>(this T target) where T : Object
        {
#if UNITY_EDITOR
            if (PrefabUtility.IsPartOfPrefabAsset(target) || Application.isPlaying)
                return;

            EditorUtility.SetDirty(target);

            if (target is not Component component)
                return;

            var scene = component.gameObject.scene;
            if (scene.isLoaded && !string.IsNullOrEmpty(scene.path))
                EditorSceneManager.MarkSceneDirty(scene);
#endif
        }
    }
}