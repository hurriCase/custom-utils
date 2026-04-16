#if FIREBASE
using System;
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
    /// <inheritdoc />
    /// <summary>
    /// Stores data using Firebase Storage. Requires the <c>FIREBASE</c> scripting define symbol.
    /// Data is scoped per user under <c>users/{userId}</c>.
    /// </summary>
    [PublicAPI]
    public sealed class FirebaseStorageProvider : BaseCloudStorageProvider<byte[]>
    {
        private const long MaxDownloadSize = 5 * 1024 * 1024;

        private readonly IBytesSerializer _serializer;
        private readonly FirebaseStorage _firebaseStorage;
        private readonly string _storagePath;

        public FirebaseStorageProvider(IBytesSerializer serializer, TimeSpan debounceDelay, string userId = null) :
            base(debounceDelay)
        {
            _serializer = serializer;
            _firebaseStorage = FirebaseStorage.DefaultInstance;

            userId = string.IsNullOrEmpty(userId) ? SystemInfo.deviceUniqueIdentifier : userId;
            _storagePath = $"users/{userId}";

            Logger.Log($"[FirebaseStorageProvider::FirebaseStorageProvider] Initialized with user ID: {userId}");
        }

        protected override byte[] Serialize<TData>(TData data) => _serializer.SerializeToBytes(data);
        protected override TData Deserialize<TData>(byte[] raw) => _serializer.DeserializeFromBytes<TData>(raw);

        private StorageReference GetFileReference(string key)
            => _firebaseStorage.GetReference($"{_storagePath}/{key}");

        protected override async UniTask PlatformSaveAsync(string key, byte[] data)
            => await GetFileReference(key).PutBytesAsync(data);

        protected override async UniTask<byte[]> PlatformLoadAsync(string key, CancellationToken token)
        {
            if (!await PlatformHasKeyAsync(key, token))
                return null;

            var bytes = await GetFileReference(key)
                .GetBytesAsync(MaxDownloadSize)
                .AsUniTask()
                .AttachExternalCancellation(token);

            return bytes == null || bytes.Length == 0 ? null : bytes;
        }

        protected override async UniTask<bool> PlatformHasKeyAsync(string key, CancellationToken token)
        {
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

        protected override async UniTask PlatformDeleteKeyAsync(string key, CancellationToken token)
        {
            if (!await PlatformHasKeyAsync(key, token))
                return;

            await GetFileReference(key).DeleteAsync().AsUniTask().AttachExternalCancellation(token);
        }

        protected override UniTask<bool> PlatformTryDeleteAllAsync(CancellationToken token)
        {
            Logger.LogWarning($"[FirebaseStorageProvider::TryDeleteAllAsync] DeleteAll is not supported");
            return UniTask.FromResult(false);
        }
    }
}
#endif