using CustomUtils.Editor.Scripts.Extensions;
using CustomUtils.Runtime.Serializer;
using CustomUtils.Runtime.Storage.Persistent;
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
        private static readonly EditorPrefsProvider _editorPrefsProvider = new(SerializerProvider.StringSerializer);

        /// <summary>
        /// Creates a persistent property that automatically saves to EditorPrefs
        /// </summary>
        /// <typeparam name="TProperty">Type of the property value</typeparam>
        /// <param name="target">Target object to create unique key for</param>
        /// <param name="key">Base key for storage</param>
        /// <param name="defaultValue">Default value if no saved value exists</param>
        /// <returns>The property holding the saved value, or <paramref name="defaultValue"/> if none exists</returns>
        public static PersistentReactiveProperty<TProperty> CreatePersistentProperty<TProperty>(
            this Object target,
            string key,
            TProperty defaultValue = default)
        {
            var uniqueKey = target
                ? target.GetObjectUniqueKey(key) ?? key
                : key;

            // EditorPrefsProvider completes synchronously, so the result is available without awaiting
            return PersistentReactiveProperty<TProperty>
                .CreateAsync(uniqueKey, defaultValue: defaultValue, provider: _editorPrefsProvider)
                .GetAwaiter()
                .GetResult();
        }
    }
}
