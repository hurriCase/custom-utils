using System.IO;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for file paths.
    /// </summary>
    [PublicAPI]
    public static class FileExtensions
    {
        /// <summary>
        /// Deletes the file at <paramref name="path"/> if it exists.
        /// </summary>
        /// <param name="path">Path of the file to delete</param>
        /// <returns>True if the file existed and was deleted, false if there was no file</returns>
        public static bool TryDeleteFile(this string path)
        {
            if (!File.Exists(path))
                return false;

            File.Delete(path);
            return true;
        }
    }
}
