using System;
using CustomUtils.Runtime.Serializer;
using UnityEditor;

namespace CustomUtils.Editor.Scripts.PersistentEditor
{
    /// <summary>
    /// Helper for saving/loading values to EditorPrefs with type support
    /// </summary>
    internal static class EditorPrefsHelper
    {
        /// <summary>
        /// Saves a value to EditorPrefs with automatic type handling
        /// </summary>
        /// <typeparam name="T">Type of value to save</typeparam>
        /// <param name="key">Storage key</param>
        /// <param name="value">Value to save</param>
        internal static void SetValue<T>(string key, T value)
        {
            switch (value)
            {
                case bool boolValue:
                    EditorPrefs.SetBool(key, boolValue);
                    break;

                case int intValue:
                    EditorPrefs.SetInt(key, intValue);
                    break;

                case float floatValue:
                    EditorPrefs.SetFloat(key, floatValue);
                    break;

                case string stringValue:
                    EditorPrefs.SetString(key, stringValue);
                    break;

                default:
                    var serialized = SerializerProvider.Serializer.Serialize(value);
                    EditorPrefs.SetString(key, Convert.ToBase64String(serialized));
                    break;
            }
        }

        /// <summary>
        /// Loads a value from EditorPrefs with automatic type handling
        /// </summary>
        /// <typeparam name="T">Type of value to load</typeparam>
        /// <param name="key">Storage key</param>
        /// <returns>Loaded value or default if not found</returns>
        internal static T GetValue<T>(string key)
        {
            var type = typeof(T);

            if (type == typeof(bool))
                return (T)(object)EditorPrefs.GetBool(key, false);
            if (type == typeof(int))
                return (T)(object)EditorPrefs.GetInt(key, 0);
            if (type == typeof(float))
                return (T)(object)EditorPrefs.GetFloat(key, 0f);
            if (type == typeof(string))
                return (T)(object)EditorPrefs.GetString(key, string.Empty);

            var serialized = EditorPrefs.GetString(key, string.Empty);

            if (string.IsNullOrEmpty(serialized))
                return default;

            try
            {
                var data = Convert.FromBase64String(serialized);
                return SerializerProvider.Serializer.Deserialize<T>(data);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Checks if a key exists in EditorPrefs
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>True if key exists, false otherwise</returns>
        internal static bool HasKey(string key) => EditorPrefs.HasKey(key);
    }
}