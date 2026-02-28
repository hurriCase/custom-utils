using System;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.AssetLoader
{
    /// <inheritdoc />
    /// <summary>
    /// Attribute for specifying the location of a resource asset.
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ResourceAttribute : Attribute
    {
        /// <summary>
        /// Gets the full path to the resource asset folder.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Gets the name of the resource asset.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the resource path used for loading with Resources.Load.
        /// </summary>
        private string ResourcePath { get; }

        /// <summary>
        /// Gets a value indicating whether this resource is in the Editor Default Resources folder.
        /// </summary>
        public bool IsEditorResource { get; }

        /// <summary>
        /// Gets the file extension for the resource (defaults to ".asset" for editor resources).
        /// </summary>
        public string Extension { get; }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CustomUtils.Runtime.AssetLoader.ResourceAttribute" /> class.
        /// </summary>
        /// <param name="fullPath">The full path to the resource asset folder.</param>
        /// <param name="name">The name of the resource asset.</param>
        /// <param name="resourcePath">The resource path used for loading with Resources.Load.</param>
        /// <param name="isEditorResource">Indicates whether this resource is in the Editor Default Resources folder.</param>
        /// <param name="extension">The file extension for the resource (e.g., ".asset", ".png", ".prefab").</param>
        public ResourceAttribute(string fullPath = null, string name = null, string resourcePath = null,
            string extension = null, bool isEditorResource = false)
        {
            FullPath = fullPath;
            Name = name;
            ResourcePath = resourcePath;
            IsEditorResource = isEditorResource;

            Extension = extension ?? (isEditorResource ? ".asset" : string.Empty);
        }

        /// <summary>
        /// Tries to get the full resource path for use with Resources.Load.
        /// </summary>
        /// <param name="fullResourcePath">When this method returns, contains the full resource path if successful; otherwise, null.</param>
        /// <returns>true if the full resource path was successfully created; otherwise, false.</returns>
        public bool TryGetFullResourcePath(ref string fullResourcePath)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                fullResourcePath = null;
                Debug.LogError("[ResourceAttribute::TryGetFullResourcePath] " +
                               $"Resource name cannot be null or empty - {Name}");

                return false;
            }

            var nameWithExtension = Name;

            if (!string.IsNullOrEmpty(Extension) && !Name.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
                nameWithExtension = $"{Name}{Extension}";

            if (IsEditorResource)
                fullResourcePath = string.IsNullOrWhiteSpace(FullPath)
                    ? nameWithExtension
                    : Path.Combine(FullPath, nameWithExtension);
            else
                fullResourcePath = string.IsNullOrWhiteSpace(ResourcePath)
                    ? Name
                    : Path.Combine(ResourcePath, Name);

            return true;
        }
    }
}