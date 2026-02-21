using JetBrains.Annotations;
using UnityEditor;

namespace CustomUtils.Editor.Scripts.Extensions
{
    /// <summary>
    /// Provides Editor time extension methods for <see cref="string"/>.
    /// </summary>
    [PublicAPI]
    public static class StringExtensionsEditor
    {
        /// <summary>
        /// Creates a folder structure in the Unity project based on the provided path.
        /// </summary>
        /// <param name="path">The path where the folders should be created, using forward slashes (/) as separators.</param>
        public static void CreateFolder(this string path)
        {
            var folders = path.Split('/');
            var currentPath = folders[0];

            for (var i = 1; i < folders.Length; i++)
            {
                var parentPath = currentPath;
                currentPath = $"{currentPath}/{folders[i]}";

                if (AssetDatabase.IsValidFolder(currentPath) is false)
                    AssetDatabase.CreateFolder(parentPath, folders[i]);
            }

            AssetDatabase.Refresh();
        }
    }
}