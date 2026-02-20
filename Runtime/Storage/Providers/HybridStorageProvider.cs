using System.Threading;
using CustomUtils.Runtime.Serializer;
using CustomUtils.Runtime.Storage.Base;
using CustomUtils.Runtime.Storage.DataTransformers;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Storage.Providers
{
    /// <summary>
    /// Hybrid storage provider with smart TryDeleteAll behavior
    /// Reads from local first, then cloud. Writes to cloud with local fallback.
    /// </summary>
    [PublicAPI]
    internal sealed class HybridStorageProvider : BaseStorageProvider
    {
        private readonly IStorageProvider _localProvider;
        private readonly IStorageProvider _cloudProvider;
        private readonly bool _autoMigrate;

        /// <summary>
        /// Creates a hybrid provider that combines local and cloud storage
        /// </summary>
        /// <param name="localProvider">Local storage provider (fallback)</param>
        /// <param name="cloudProvider">Cloud storage provider (primary)</param>
        /// <param name="autoMigrate">Whether to automatically migrate local data to cloud</param>
        public HybridStorageProvider(IStorageProvider localProvider, IStorageProvider cloudProvider,
            bool autoMigrate = true) : base(new IdentityDataTransformer(), SerializerProvider.Serializer)
        {
            _localProvider = localProvider;
            _cloudProvider = cloudProvider;
            _autoMigrate = autoMigrate;
        }

        protected override async UniTask PlatformSaveAsync(string key, object transformData, CancellationToken token)
        {
            if (await _cloudProvider.TrySaveAsync(key, transformData, token))
                if (_autoMigrate)
                {
                    await _localProvider.TryDeleteKeyAsync(key, token);
                    return;
                }

#if IS_TEST
            Debug.LogWarning("[HybridStorageProvider] " +
                             $"Cloud save failed for key '{key}'. Falling back to local.");
#endif

            await _localProvider.TrySaveAsync(key, transformData, token);
        }

        protected override async UniTask<object> PlatformLoadAsync(string key, CancellationToken token)
        {
            if (await _cloudProvider.HasKeyAsync(key, token))
                return await _cloudProvider.LoadAsync<object>(key, token);

            if (await _localProvider.HasKeyAsync(key, token) is false)
                return null;

            var localData = await _localProvider.LoadAsync<object>(key, token);

            if (_autoMigrate is false || localData == null)
                return localData;

            if (await _cloudProvider.TrySaveAsync(key, localData, token))
                await _localProvider.TryDeleteKeyAsync(key, token);

#if IS_TEST
            Debug.Log($"[HybridStorageProvider::PlatformLoadAsync] Auto-migrated key '{key}' to cloud storage");
#endif

            return localData;
        }

        protected override async UniTask<bool> PlatformHasKeyAsync(string key, CancellationToken token)
        {
            if (await _cloudProvider.HasKeyAsync(key, token))
                return true;

            return await _localProvider.HasKeyAsync(key, token);
        }

        protected override async UniTask PlatformDeleteKeyAsync(string key, CancellationToken token)
        {
            await _cloudProvider.TryDeleteKeyAsync(key, token);
            await _localProvider.TryDeleteKeyAsync(key, token);
        }

        protected override async UniTask<bool> PlatformTryDeleteAllAsync(CancellationToken token)
        {
            var localSuccess = await _localProvider.TryDeleteAllAsync(token);
            var cloudSuccess = await _cloudProvider.TryDeleteAllAsync(token);

            var overallSuccess = localSuccess || cloudSuccess;
            return overallSuccess;
        }
    }
}