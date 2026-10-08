using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace CustomUtils.Runtime.Storage.Base
{
    public abstract class StorageProviderBase<TCached> : IStorageProvider
    {
        private readonly Dictionary<string, TCached> _cache = new();

        public virtual async UniTask<bool> TrySaveAsync<TData>(string key, TData data, bool isForce = false)
        {
            try
            {
                var cached = Serialize(data);
                _cache[key] = cached;
                await PlatformSaveAsync(key, cached);
                Logger.Log($"[{GetType().Name}::TrySaveAsync] Saved data for key '{key}'");
                return true;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _cache.Remove(key);

                Logger.LogException(exception);
                Logger.LogError($"[{GetType().Name}::TrySaveAsync] Error during saving data: {exception.Message}");
                return false;
            }
        }

        public async UniTask<LoadResult<TData>> TryLoadAsync<TData>(string key, CancellationToken token)
        {
            try
            {
                if (_cache.TryGetValue(key, out var cached))
                    return LoadResult<TData>.Loaded(Deserialize<TData>(cached));

                var raw = await PlatformLoadAsync(key, token);
                if (raw == null)
                    return LoadResult<TData>.NotFound;

                var data = Deserialize<TData>(raw);
                _cache[key] = raw;
                Logger.Log($"[{GetType().Name}::{nameof(TryLoadAsync)}] Loaded data for key '{key}'");
                return LoadResult<TData>.Loaded(data);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Logger.LogException(exception);
                Logger.LogError($"[{GetType().Name}::{nameof(TryLoadAsync)}] " +
                                $"Error loading data for key '{key}': {exception.Message}");
                return LoadResult<TData>.Failed;
            }
        }

        public async UniTask<bool> HasKeyAsync(string key, CancellationToken token)
        {
            if (_cache.ContainsKey(key))
                return true;

            return await PlatformHasKeyAsync(key, token);
        }

        public async UniTask<bool> TryDeleteKeyAsync(string key, CancellationToken token)
        {
            try
            {
                _cache.Remove(key);
                await PlatformDeleteKeyAsync(key, token);
                Logger.Log($"[{GetType().Name}::TryDeleteKeyAsync] Deleted key '{key}'");
                return true;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Logger.LogException(exception);
                Logger.LogError($"[{GetType().Name}::TryDeleteKeyAsync] Error deleting key: {exception.Message}");
                return false;
            }
        }

        public virtual async UniTask<bool> TryDeleteAllAsync(CancellationToken token)
        {
            try
            {
                _cache.Clear();
                var success = await PlatformTryDeleteAllAsync(token);
                if (success)
                    Logger.Log($"[{GetType().Name}::TryDeleteAllAsync] Successfully deleted all data");
                else
                    Logger.LogWarning($"[{GetType().Name}::TryDeleteAllAsync] DeleteAll not supported or failed");
                return success;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Logger.LogException(exception);
                Logger.LogError($"[{GetType().Name}::TryDeleteAllAsync] Error deleting all data: {exception.Message}");
                return false;
            }
        }

        protected abstract TCached Serialize<TData>(TData data);
        protected abstract TData Deserialize<TData>(TCached raw);
        protected abstract UniTask PlatformSaveAsync(string key, TCached data);
        protected abstract UniTask<TCached> PlatformLoadAsync(string key, CancellationToken token);
        protected abstract UniTask<bool> PlatformHasKeyAsync(string key, CancellationToken token);
        protected abstract UniTask PlatformDeleteKeyAsync(string key, CancellationToken token);
        protected abstract UniTask<bool> PlatformTryDeleteAllAsync(CancellationToken token);
    }
}