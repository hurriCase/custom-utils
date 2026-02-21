using CustomUtils.Runtime.Formatter;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.Extensions
{
    /// <summary>
    /// Provides Editor time extension methods for <see cref="Object"/>.
    /// </summary>
    [PublicAPI]
    public static class ObjectExtensionsEditor
    {
        /// <summary>
        /// Generates a unique key for a Unity Object by combining a base key with the object's global identifier.
        /// </summary>
        /// <param name="target">The Unity Object to generate a unique key for. Must not be null.</param>
        /// <param name="baseKey">The base string to prefix the unique identifier with.</param>
        /// <returns>
        /// A unique string key in the format "{baseKey}.{hash}" where hash is either:
        /// – The hash code of the GlobalObjectId string representation (preferred), or
        /// – The instance ID as fallback if GlobalObjectId parsing fails.
        /// </returns>
        /// <remarks>
        /// This method attempts to use Unity's GlobalObjectId for maximum uniqueness across sessions.
        /// If GlobalObjectId parsing fails, it falls back to using the instance ID, which is unique
        /// within the current session but may not persist across Unity sessions.
        /// </remarks>
        [MustUseReturnValue]
        public static string GetObjectUniqueKey([NotNull] this Object target, string baseKey)
        {
            var objectId = GlobalObjectId.TryParse(GlobalObjectId.GetGlobalObjectIdSlow(target).ToString(),
                out var globalId)
                ? globalId.ToString().GetHashCode()
                : target.GetInstanceID();

            return StringFormatter.Format("{0}.{1}", baseKey, objectId);
        }
    }
}