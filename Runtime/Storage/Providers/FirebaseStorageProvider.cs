#if FIREBASE
using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Serializer;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using Firebase.Extensions;
using Firebase.Storage;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Storage.Providers
{
    [PublicAPI]
    public sealed class FirebaseStorageProvider : BaseCloudStorageProvider
    {
        private const long MaxDownloadSize = 5 * 1024 * 1024;

        private readonly IBytesSerializer _serializer;
        private readonly FirebaseStorage _firebaseStorage;
        private readonly string _storagePath;
        private readonly Dictionary<string, byte[]> _cache = new();

        public FirebaseStorageProvider(IBytesSerializer serializer, TimeSpan debounceDelay, string userId = null) :
            base(debounceDelay)
        {
            _serializer = serializer;
            _firebaseStorage = FirebaseStorage.DefaultInstance;

            userId = string.IsNullOrEmpty(userId) ? SystemInfo.deviceUniqueIdentifier : userId;
            _storagePath = $"users/{userId}";

            Logger.Log($"[FirebaseStorageProvider::FirebaseStorageProvider] Initialized with user ID: {userId}");
        }

        private StorageReference GetFileReference(string key)
            => _firebaseStorage.GetReference($"{_storagePath}/{key}");

        protected override async UniTask<bool> OnTrySaveAsync<TData>(string key, TData data)
        {
            try
            {
                var bytes = _serializer.SerializeToBytes(data);
                _cache[key] = bytes;

                var reference = GetFileReference(key);
                await reference.PutBytesAsync(bytes);

                Logger.Log($"[FirebaseStorageProvider::OnTrySaveAsync] Saved data for key '{key}'");
                return true;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.LogError(
                    $"[FirebaseStorageProvider::OnTrySaveAsync] Error saving data for key '{key}': {e.Message}");
                return false;
            }
        }

        public override async UniTask<TData> LoadAsync<TData>(string key, CancellationToken token)
        {
            try
            {
                if (_cache.TryGetValue(key, out var cachedBytes))
                    return _serializer.DeserializeFromBytes<TData>(cachedBytes);

                if (!await HasKeyAsync(key, token))
                    return default;

                var bytes = await GetFileReference(key)
                    .GetBytesAsync(MaxDownloadSize)
                    .AsUniTask()
                    .AttachExternalCancellation(token);

                if (bytes == null || bytes.Length == 0)
                    return default;

                _cache[key] = bytes;
                var data = _serializer.DeserializeFromBytes<TData>(bytes);

                Logger.Log($"[FirebaseStorageProvider::LoadAsync] Loaded data for key '{key}'");
                return data;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.LogError(
                    $"[FirebaseStorageProvider::LoadAsync] Error loading data for key '{key}': {e.Message}");
                return default;
            }
        }

        public override async UniTask<bool> HasKeyAsync(string key, CancellationToken token)
        {
            if (_cache.ContainsKey(key))
                return true;

            try
            {
                return await GetFileReference(key)
                    .GetMetadataAsync()
                    .ContinueWithOnMainThread(static task => !task.IsFaulted && !task.IsCanceled)
                    .AsUniTask()
                    .AttachExternalCancellation(token);
            }
            catch
            {
                return false;
            }
        }

        public override async UniTask<bool> TryDeleteKeyAsync(string key, CancellationToken token)
        {
            try
            {
                _cache.Remove(key);

                if (!await HasKeyAsync(key, token))
                    return true;

                await GetFileReference(key).DeleteAsync().AsUniTask().AttachExternalCancellation(token);

                Logger.Log($"[FirebaseStorageProvider::TryDeleteKeyAsync] Deleted key '{key}'");
                return true;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.LogError(
                    $"[FirebaseStorageProvider::TryDeleteKeyAsync] Error deleting key '{key}': {e.Message}");
                return false;
            }
        }

        public override UniTask<bool> TryDeleteAllAsync(CancellationToken token)
        {
            Logger.LogWarning($"[FirebaseStorageProvider::TryDeleteAllAsync] DeleteAll is not supported");
            return UniTask.FromResult(false);
        }
    }
}
#endif