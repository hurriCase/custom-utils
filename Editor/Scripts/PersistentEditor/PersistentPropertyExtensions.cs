using CustomUtils.Editor.Scripts.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.PersistentEditor
{
    /// <summary>
    /// Extension methods for creating persistent editor properties
    /// </summary>
    [PublicAPI]
    public static class PersistentPropertyExtensions
    {
        /// <summary>
        /// Creates a persistent editor property that automatically saves to EditorPrefs
        /// </summary>
        /// <typeparam name="TProperty">Type of the property value</typeparam>
        /// <param name="target">Target object to create unique key for</param>
        /// <param name="key">Base key for storage</param>
        /// <param name="defaultValue">Default value if no saved value exists</param>
        /// <returns>New persistent editor property</returns>
        public static PersistentEditorProperty<TProperty> CreatePersistentProperty<TProperty>(
            this Object target,
            string key,
            TProperty defaultValue = default)
        {
            var uniqueKey = target
                ? target.GetObjectUniqueKey(key) ?? key
                : key;

            return new PersistentEditorProperty<TProperty>(uniqueKey, defaultValue);
        }
    }
}