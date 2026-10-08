using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Storage.Providers
{
    /// <inheritdoc />
    /// <summary>
    /// Keeps a permanent local copy of every value and mirrors it to the cloud.
    /// The local copy is the source of truth, so the game works offline. The cloud is read only when there's
    /// no local copy (first launch on a new device or after a reinstall).
    /// </summary>
    /// <remarks>
    /// Keys that changed locally but haven't reached the cloud yet are kept in a pending set, stored locally
    /// so it survives restarts. A pending key is uploaded again the next time it's saved or loaded.
    /// </remarks>
    [PublicAPI]
    public sealed class HybridStorageProvider : IStorageProvider
    {
        private const string PendingKeysKey = "__hybrid_pending_keys";

        private readonly IStorageProvider _localProvider;
        private readonly ICloudStorageProvider _cloudProvider;
        private readonly AsyncLazy<HashSet<string>> _pendingKeys;

        private readonly Dictionary<string, int> _saveVersions = new();

        /// <summary>
        /// Creates a provider that stores data locally and mirrors it to the cloud.
        /// </summary>
        /// <param name="localProvider">Device storage that holds the permanent copy</param>
        /// <param name="cloudProvider">Remote storage the local copy is mirrored to</param>
        public HybridStorageProvider(IStorageProvider localProvider, ICloudStorageProvider cloudProvider)
        {
            _localProvider = localProvider;
            _cloudProvider = cloudProvider;
            _pendingKeys = UniTask.Lazy(LoadPendingKeysAsync);
        }

        public async UniTask<bool> TrySaveAsync<T>(string key, T data, bool isForce = false)
        {
            if (!await _localProvider.TrySaveAsync(key, data, isForce))
                return false;

            _saveVersions[key] = GetSaveVersion(key) + 1;
            await MarkPendingAsync(key);
            await UploadAsync(key, data, isForce);

            return true;
        }

        public async UniTask<LoadResult<T>> TryLoadAsync<T>(string key, CancellationToken token = default)
        {
            var localResult = await _localProvider.TryLoadAsync<T>(key, token);

            if (localResult.Status == LoadStatus.Loaded)
            {
                if ((await _pendingKeys).Contains(key))
                    UploadAsync(key, localResult.Data, isForce: true).Forget();

                return localResult;
            }

            var cloudResult = await _cloudProvider.TryLoadAsync<T>(key, token);

            if (cloudResult.Status == LoadStatus.Loaded)
                await _localProvider.TrySaveAsync(key, cloudResult.Data);

            return cloudResult;
        }

        public async UniTask<bool> HasKeyAsync(string key, CancellationToken token = default)
        {
            if (await _localProvider.HasKeyAsync(key, token))
                return true;

            return await _cloudProvider.HasKeyAsync(key, token);
        }

        public async UniTask<bool> TryDeleteKeyAsync(string key, CancellationToken token = default)
        {
            var localSuccess = await _localProvider.TryDeleteKeyAsync(key, token);
            var cloudSuccess = await _cloudProvider.TryDeleteKeyAsync(key, token);

            if (cloudSuccess)
                await UnmarkPendingAsync(key);

            return localSuccess || cloudSuccess;
        }

        public async UniTask<bool> TryDeleteAllAsync(CancellationToken token = default)
        {
            var localSuccess = await _localProvider.TryDeleteAllAsync(token);
            var cloudSuccess = await _cloudProvider.TryDeleteAllAsync(token);

            if (localSuccess)
                (await _pendingKeys).Clear();

            return localSuccess || cloudSuccess;
        }

        private async UniTask UploadAsync<T>(string key, T data, bool isForce)
        {
            var version = GetSaveVersion(key);

            if (await _cloudProvider.TrySaveAsync(key, data, isForce) && GetSaveVersion(key) == version)
                await UnmarkPendingAsync(key);
        }

        private int GetSaveVersion(string key) => _saveVersions.GetValueOrDefault(key, 0);

        private async UniTask MarkPendingAsync(string key)
        {
            var pendingKeys = await _pendingKeys;

            if (pendingKeys.Add(key))
                await _localProvider.TrySaveAsync(PendingKeysKey, pendingKeys);
        }

        private async UniTask UnmarkPendingAsync(string key)
        {
            var pendingKeys = await _pendingKeys;

            if (pendingKeys.Remove(key))
                await _localProvider.TrySaveAsync(PendingKeysKey, pendingKeys);
        }

        private async UniTask<HashSet<string>> LoadPendingKeysAsync()
        {
            var result = await _localProvider.TryLoadAsync<HashSet<string>>(PendingKeysKey);

            return result is { Status: LoadStatus.Loaded, Data: not null }
                ? result.Data
                : new HashSet<string>();
        }
    }
}
