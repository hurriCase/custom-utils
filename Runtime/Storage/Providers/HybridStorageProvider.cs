using System.Threading;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Storage.Providers
{
    /// <inheritdoc />
    /// <summary>
    /// A storage provider that writes to cloud with automatic local fallback.
    /// On load, migrates local data to cloud if <c>autoMigrate</c> is enabled.
    /// </summary>
    [PublicAPI]
    public sealed class HybridStorageProvider : IStorageProvider
    {
        private readonly IStorageProvider _localProvider;
        private readonly IStorageProvider _cloudProvider;
        private readonly bool _autoMigrate;

        /// <summary>
        /// Creates a hybrid provider combining local and cloud storage.
        /// </summary>
        /// <param name="localProvider">Fallback provider used when cloud is unavailable.</param>
        /// <param name="cloudProvider">Primary provider for cloud persistence.</param>
        /// <param name="autoMigrate">
        /// When true, local data is migrated to cloud on load and deleted locally after a successful cloud save.
        /// </param>
        public HybridStorageProvider(
            IStorageProvider localProvider,
            IStorageProvider cloudProvider,
            bool autoMigrate = true)
        {
            _localProvider = localProvider;
            _cloudProvider = cloudProvider;
            _autoMigrate = autoMigrate;
        }

        public async UniTask<bool> TrySaveAsync<T>(string key, T data, CancellationToken token = default)
        {
            if (!await _cloudProvider.TrySaveAsync(key, data, token))
                return await _localProvider.TrySaveAsync(key, data, token);

            if (_autoMigrate)
                await _localProvider.TryDeleteKeyAsync(key, token);

            return true;
        }

        public async UniTask<T> LoadAsync<T>(string key, CancellationToken token = default)
        {
            if (await _cloudProvider.HasKeyAsync(key, token))
                return await _cloudProvider.LoadAsync<T>(key, token);

            if (!await _localProvider.HasKeyAsync(key, token))
                return default;

            var localData = await _localProvider.LoadAsync<T>(key, token);

            if (!_autoMigrate || localData == null)
                return localData;

            if (await _cloudProvider.TrySaveAsync(key, localData, token))
                await _localProvider.TryDeleteKeyAsync(key, token);

            return localData;
        }

        public async UniTask<bool> HasKeyAsync(string key, CancellationToken token = default)
        {
            if (await _cloudProvider.HasKeyAsync(key, token))
                return true;
            return await _localProvider.HasKeyAsync(key, token);
        }

        public async UniTask<bool> TryDeleteKeyAsync(string key, CancellationToken token = default)
        {
            await _cloudProvider.TryDeleteKeyAsync(key, token);
            await _localProvider.TryDeleteKeyAsync(key, token);
            return true;
        }

        public async UniTask<bool> TryDeleteAllAsync(CancellationToken token = default)
        {
            var localSuccess = await _localProvider.TryDeleteAllAsync(token);
            var cloudSuccess = await _cloudProvider.TryDeleteAllAsync(token);
            return localSuccess || cloudSuccess;
        }
    }
}