using System.IO;
using System.Threading;
using CustomUtils.Runtime.Serializer;
using CustomUtils.Runtime.Storage.Base;
using CustomUtils.Runtime.Storage.DataTransformers;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Storage.Providers
{
    /// <summary>
    /// Binary file provider with TryDeleteAll support
    /// </summary>
    [PublicAPI]
    internal sealed class BinaryFileProvider : BaseStorageProvider
    {
        private readonly string _saveDirectory;

        internal BinaryFileProvider() : base(new IdentityDataTransformer(), SerializerProvider.Serializer)
        {
            _saveDirectory = Path.Combine(Application.persistentDataPath, "SaveData");

            if (Directory.Exists(_saveDirectory) is false)
                Directory.CreateDirectory(_saveDirectory);
        }

        private string GetFilePath(string key) => Path.Combine(_saveDirectory, $"{key}.dat");

        protected override async UniTask PlatformSaveAsync(string key, object transformData, CancellationToken token)
        {
            if (transformData is byte[] byteData)
                await File.WriteAllBytesAsync(GetFilePath(key), byteData, token);
        }

        protected override async UniTask<object> PlatformLoadAsync(string key, CancellationToken token)
        {
            var filePath = GetFilePath(key);

            if (File.Exists(filePath) is false)
                return null;

            var result = await File.ReadAllBytesAsync(filePath, token);
            return result;
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

        protected override UniTask<bool> PlatformTryDeleteAllAsync(CancellationToken token)
        {
            return UniTask.RunOnThreadPool(() =>
                {
                    if (Directory.Exists(_saveDirectory) is false)
                        return true;

                    Directory.Delete(_saveDirectory, true);
                    Directory.CreateDirectory(_saveDirectory);
                    return true;
                }, configureAwait: true,
                cancellationToken: token);
        }
    }
}