using System;
using UnityEngine;

namespace CustomUtils.Runtime.AssetLoader
{
    internal static class PathUtility
    {
        internal static bool TryGetResourcePath<TResource>(ref string resourcePath)
        {
            if (!string.IsNullOrWhiteSpace(resourcePath))
                return ValidateResourcePath(ref resourcePath);

            var type = typeof(TResource);

            if (Attribute.GetCustomAttribute(type, typeof(ResourceAttribute)) is ResourceAttribute attribute)
            {
                resourcePath = attribute.TryGetFullResourcePath(ref resourcePath)
                    ? resourcePath
                    : typeof(TResource).Name;

                return ValidateResourcePath(ref resourcePath);
            }

            Debug.LogWarning($"[ResourceLoader::GetPath] No ResourceAttribute found on type {type.Name}");
            return false;
        }

        private static bool ValidateResourcePath(ref string resourcePath)
        {
            if (resourcePath.Contains(".."))
            {
                Debug.LogError("[ResourceAttribute::TryGetFullResourcePath] " +
                               $"Resource path cannot contain parent directory references: {resourcePath}");
                return false;
            }

            resourcePath.TrimStart('/');
            return true;
        }
    }
}