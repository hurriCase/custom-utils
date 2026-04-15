using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CustomUtils.Runtime.Serializer;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Storage.Providers
{
    [PublicAPI]
    public sealed class BinaryFileProvider : BaseStorageProvider
    {
        private readonly IBytesSerializer _serializer;
        private readonly Dictionary<string, byte[]> _cache = new();
        private readonly string _saveDirectory;

        private const string SaveFolderName = "SaveData";
        private const string SaveFileExtension = "dat";

        public BinaryFileProvider(IBytesSerializer serializer)
        {
            _serializer = serializer;
            _saveDirectory = Path.Combine(Application.persistentDataPath, SaveFolderName);

            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);
        }

        public override async UniTask<bool> TrySaveAsync<TData>(string key, TData data, bool isForce = false)
        {
            try
            {
                var bytes = _serializer.SerializeToBytes(data);
                _cache[key] = bytes;
                await File.WriteAllBytesAsync(GetFilePath(key), bytes);
                Logger.Log($"[{nameof(BinaryFileProvider)}::TrySaveAsync] Saved data for key '{key}'");
                return true;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.LogError($"[{nameof(BinaryFileProvider)}::TrySaveAsync] Error during saving data: {e.Message}");
                return false;
            }
        }

        public override async UniTask<TData> LoadAsync<TData>(string key, CancellationToken token)
        {
            try
            {
                if (_cache.TryGetValue(key, out var cachedBytes))
                    return _serializer.DeserializeFromBytes<TData>(cachedBytes);

                var filePath = GetFilePath(key);
                if (!File.Exists(filePath))
                    return default;

                var bytes = await File.ReadAllBytesAsync(filePath, token);
                _cache[key] = bytes;
                var data = _serializer.DeserializeFromBytes<TData>(bytes);
                Logger.Log($"[{nameof(BinaryFileProvider)}::LoadAsync] Loaded data for key '{key}'");
                return data;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.LogError($"[{nameof(BinaryFileProvider)}::LoadAsync] Error loading data: {e.Message}");
                return default;
            }
        }

        public override UniTask<bool> HasKeyAsync(string key, CancellationToken token)
            => UniTask.FromResult(_cache.ContainsKey(key) || File.Exists(GetFilePath(key)));

        public override async UniTask<bool> TryDeleteKeyAsync(string key, CancellationToken token)
        {
            try
            {
                _cache.Remove(key);
                var filePath = GetFilePath(key);
                await UniTask.RunOnThreadPool(static path =>
                {
                    if (File.Exists((string)path)) File.Delete((string)path);
                }, filePath, configureAwait: true, cancellationToken: token);
                Logger.Log($"[{nameof(BinaryFileProvider)}::TryDeleteKeyAsync] Deleted key '{key}'");
                return true;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.LogError($"[{nameof(BinaryFileProvider)}::TryDeleteKeyAsync] Error deleting key: {e.Message}");
                return false;
            }
        }

        public override UniTask<bool> TryDeleteAllAsync(CancellationToken token) =>
            UniTask.RunOnThreadPool(RecreateSaveFolder, configureAwait: true, cancellationToken: token);

        private string GetFilePath(string key) => Path.Combine(_saveDirectory, $"{key}.{SaveFileExtension}");

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