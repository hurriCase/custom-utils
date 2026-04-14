using System.Threading;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Storage.Providers
{
    [PublicAPI]
    public sealed class HybridStorageProvider : IStorageProvider
    {
        private readonly IStorageProvider _localProvider;
        private readonly IStorageProvider _cloudProvider;
        private readonly bool _autoMigrate;

        public HybridStorageProvider(
            IStorageProvider localProvider,
            IStorageProvider cloudProvider,
            bool autoMigrate = true)
        {
            _localProvider = localProvider;
            _cloudProvider = cloudProvider;
            _autoMigrate = autoMigrate;
        }

        public async UniTask<bool> TrySaveAsync<T>(string key, T data)
        {
            if (!await _cloudProvider.TrySaveAsync(key, data))
                return await _localProvider.TrySaveAsync(key, data);

            if (_autoMigrate)
                await _localProvider.TryDeleteKeyAsync(key);

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

            if (await _cloudProvider.TrySaveAsync(key, localData))
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
            var localSuccess = await _cloudProvider.TryDeleteKeyAsync(key, token);
            var cloudSuccess = await _localProvider.TryDeleteKeyAsync(key, token);
            return localSuccess || cloudSuccess;
        }

        public async UniTask<bool> TryDeleteAllAsync(CancellationToken token = default)
        {
            var localSuccess = await _localProvider.TryDeleteAllAsync(token);
            var cloudSuccess = await _cloudProvider.TryDeleteAllAsync(token);
            return localSuccess || cloudSuccess;
        }
    }
}