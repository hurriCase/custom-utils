#if FIREBASE
using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Serializer;
using CustomUtils.Runtime.Storage.Base;
using CustomUtils.Runtime.Storage.DataTransformers;
using Cysharp.Threading.Tasks;
using Firebase.Extensions;
using Firebase.Storage;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Storage.Providers
{
    /// <inheritdoc />
    /// <summary>
    /// Stores data using Firebase Storage. Requires the <c>FIREBASE</c> scripting define symbol.
    /// Data is scoped per user under <c>users/{userId}</c>.
    /// </summary>
    [PublicAPI]
    public sealed class FirebaseStorageProvider : BaseStorageProvider
    {
        private const long MaxDownloadSize = 5 * 1024 * 1024;

        private readonly FirebaseStorage _firebaseStorage;
        private readonly string _storagePath;
        private readonly Dictionary<string, byte[]> _memoryCache = new();

        public FirebaseStorageProvider(string userId = null) :
            base(new IdentityDataTransformer(), SerializerProvider.Serializer)
        {
            _firebaseStorage = FirebaseStorage.DefaultInstance;

            userId = string.IsNullOrEmpty(userId) ? SystemInfo.deviceUniqueIdentifier : userId;

            _storagePath = $"users/{userId}";

            Logger.Log($"[FirebaseStorageProvider::FirebaseStorageProvider] Initialized with user ID: {userId}");
        }

        private StorageReference GetFileReference(string key)
            => _firebaseStorage.GetReference($"{_storagePath}/{key}");

        protected override async UniTask PlatformSaveAsync(string key, object transformData, CancellationToken token)
        {
            if (!TryGetTransformedData<byte[]>(transformData, out var byteData))
                return;

            try
            {
                _memoryCache[key] = byteData;

                var reference = GetFileReference(key);

                await reference.PutBytesAsync(byteData, cancelToken: token)
                    .AsUniTask().AttachExternalCancellation(token);
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.LogError("[FirebaseStorageProvider::PlatformSaveAsync] " +
                                $"Error saving data for key {key}: {e.Message}");
            }
        }

        protected override async UniTask<object> PlatformLoadAsync(string key, CancellationToken token)
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out var cachedBytes))
                    return cachedBytes;

                var reference = GetFileReference(key);

                if (!await PlatformHasKeyAsync(key, token))
                    return null;

                var downloadTask = await reference.GetBytesAsync(MaxDownloadSize)
                    .AsUniTask().AttachExternalCancellation(token);

                if (downloadTask == null || downloadTask.Length == 0)
                    return null;

                _memoryCache[key] = downloadTask;

                return downloadTask;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.LogError("[FirebaseStorageProvider::PlatformLoadAsync] " +
                                $"Error loading data for key {key}: {e.Message}");
                return null;
            }
        }

        protected override async UniTask<bool> PlatformHasKeyAsync(string key, CancellationToken token)
        {
            if (_memoryCache.ContainsKey(key))
                return true;

            try
            {
                var reference = GetFileReference(key);

                var result = await reference.GetMetadataAsync()
                    .ContinueWithOnMainThread(static task => !task.IsFaulted && !task.IsCanceled)
                    .AsUniTask()
                    .AttachExternalCancellation(token);

                return result;
            }
            catch
            {
                return false;
            }
        }

        protected override async UniTask PlatformDeleteKeyAsync(string key, CancellationToken token)
        {
            try
            {
                _memoryCache.Remove(key);

                var reference = GetFileReference(key);

                if (!await PlatformHasKeyAsync(key, token))
                    return;

                await reference.DeleteAsync().AsUniTask().AttachExternalCancellation(token);
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.LogError("[FirebaseStorageProvider::PlatformDeleteKeyAsync] " +
                                $"Error deleting key {key}: {e.Message}");
            }
        }

        protected override UniTask<bool> PlatformTryDeleteAllAsync(CancellationToken token) =>
            UniTask.FromResult(false);
    }
}
#endif