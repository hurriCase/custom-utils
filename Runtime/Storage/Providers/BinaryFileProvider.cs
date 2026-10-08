using System.IO;
using System.Threading;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Formatter;
using CustomUtils.Runtime.Serializer;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Storage.Providers
{
    /// <inheritdoc />
    /// <summary>
    /// Stores data as binary files in <see cref="P:UnityEngine.Application.persistentDataPath">UnityEngine.Application.persistentDataPath</see>.
    /// Recommended for Android builds where PlayerPrefs may be unreliable.
    /// </summary>
    /// <remarks>
    /// File operations are synchronous on purpose. A save made while the app is being paused or closed
    /// must finish before the player loop stops, and synchronous writes also can't overlap each other.
    /// Saves are batched to at most one per key per frame, so the cost stays small.
    /// </remarks>
    [PublicAPI]
    public sealed class BinaryFileProvider : StorageProviderBase<byte[]>
    {
        private readonly IBytesSerializer _serializer;
        private readonly string _saveDirectory;

        private static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();

        private const string SaveFolderName = "SaveData";
        private const string SaveFileExtension = "dat";
        private const string TempFileExtension = "tmp";

        /// <summary>
        /// Creates a provider that stores each key as a file in the save folder.
        /// </summary>
        /// <param name="serializer">Serializer used to convert values to and from bytes</param>
        public BinaryFileProvider(IBytesSerializer serializer)
        {
            _serializer = serializer;
            _saveDirectory = Path.Combine(Application.persistentDataPath, SaveFolderName);

            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);
        }

        protected override byte[] Serialize<TData>(TData data) => _serializer.SerializeToBytes(data);
        protected override TData Deserialize<TData>(byte[] raw) => _serializer.DeserializeFromBytes<TData>(raw);

        protected override UniTask PlatformSaveAsync(string key, byte[] data)
        {
            var filePath = GetFilePath(key);
            var tempPath = GetTempPath(filePath);

            File.WriteAllBytes(tempPath, data);

            filePath.TryDeleteFile();
            File.Move(tempPath, filePath);

            return UniTask.CompletedTask;
        }

        protected override UniTask<byte[]> PlatformLoadAsync(string key, CancellationToken token)
        {
            var filePath = GetFilePath(key);

            RecoverTempFile(filePath);

            return UniTask.FromResult(File.Exists(filePath) ? File.ReadAllBytes(filePath) : null);
        }

        protected override UniTask<bool> PlatformHasKeyAsync(string key, CancellationToken token)
        {
            var filePath = GetFilePath(key);
            return UniTask.FromResult(File.Exists(filePath) || File.Exists(GetTempPath(filePath)));
        }

        protected override UniTask PlatformDeleteKeyAsync(string key, CancellationToken token)
        {
            var filePath = GetFilePath(key);

            filePath.TryDeleteFile();
            GetTempPath(filePath).TryDeleteFile();

            return UniTask.CompletedTask;
        }

        protected override UniTask<bool> PlatformTryDeleteAllAsync(CancellationToken token)
        {
            if (Directory.Exists(_saveDirectory))
                Directory.Delete(_saveDirectory, true);

            Directory.CreateDirectory(_saveDirectory);
            return UniTask.FromResult(true);
        }

        // A temp file without its main file means the app died after deleting the old file
        // but before moving the new one in. The temp file is complete in that case, so it becomes the main file.
        // A temp file next to an existing main file is a leftover from an interrupted write and is discarded.
        private void RecoverTempFile(string filePath)
        {
            var tempPath = GetTempPath(filePath);
            if (!File.Exists(tempPath))
                return;

            if (File.Exists(filePath))
                File.Delete(tempPath);
            else
                File.Move(tempPath, filePath);
        }

        private string GetFilePath(string key)
            => Path.Combine(_saveDirectory, $"{EscapeFileName(key)}.{SaveFileExtension}");

        private string GetTempPath(string filePath) => $"{filePath}.{TempFileExtension}";

        private string EscapeFileName(string key)
        {
            if (key.IndexOfAny(_invalidFileNameChars) < 0)
                return key;

            var escapedKey = key;

            foreach (var invalidChar in _invalidFileNameChars)
            {
                if (escapedKey.IndexOf(invalidChar) < 0)
                    continue;

                var invalidCharCode = StringFormatter.Format("%{0:X2}", (int)invalidChar);
                escapedKey = escapedKey.Replace(invalidChar.ToString(), invalidCharCode);
            }

            return escapedKey;
        }
    }
}
