using System.IO;
using System.Threading;
using CustomUtils.Runtime.Storage.Base;
using CustomUtils.Runtime.Storage.DataTransformers;
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
    [PublicAPI]
    public sealed class BinaryFileProvider : BaseStorageProvider
    {
        private readonly string _saveDirectory;

        private const string SaveFolderName = "SaveData";
        private const string SaveFileExtension = "dat";

        public BinaryFileProvider() : base(new IdentityDataTransformer())
        {
            _saveDirectory = Path.Combine(Application.persistentDataPath, SaveFolderName);

            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);
        }

        private string GetFilePath(string key) => Path.Combine(_saveDirectory, $"{key}.{SaveFileExtension}");

        protected override async UniTask PlatformSaveAsync(string key, object transformData)
        {
            if (TryGetTransformedData<byte[]>(transformData, out var byteData))
                await File.WriteAllBytesAsync(GetFilePath(key), byteData);
        }

        protected override async UniTask<object> PlatformLoadAsync(string key, CancellationToken token)
        {
            var filePath = GetFilePath(key);

            if (!File.Exists(filePath))
                return null;

            return await File.ReadAllBytesAsync(filePath, token);
        }

        protected override UniTask<bool> PlatformHasKeyAsync(string key, CancellationToken token)
            => UniTask.FromResult(File.Exists(GetFilePath(key)));

        protected override UniTask PlatformDeleteKeyAsync(string key, CancellationToken token)
        {
            var filePath = GetFilePath(key);

            return UniTask.RunOnThreadPool(
                static path =>
                {
                    if (File.Exists((string)path))
                        File.Delete((string)path);
                },
                filePath,
                configureAwait: true,
                cancellationToken: token);
        }

        protected override UniTask<bool> PlatformTryDeleteAllAsync(CancellationToken token) =>
            UniTask.RunOnThreadPool(RecreateSaveFolder, configureAwait: true, cancellationToken: token);

        private bool RecreateSaveFolder()
        {
            if (!Directory.Exists(_saveDirectory))
                return true;

            Directory.Delete(_saveDirectory, true);
            Directory.CreateDirectory(_saveDirectory);
            return true;
        }
    }
}