using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Serializer;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CustomUtils.Runtime.Storage.Base
{
    internal abstract class BaseStorageProvider : IStorageProvider
    {
        private readonly Dictionary<string, byte[]> _cache = new();

        private readonly IDataTransformer _dataTransformer;
        private readonly ISerializer _serializer;

        protected BaseStorageProvider(IDataTransformer dataTransformer, ISerializer serializer)
        {
            _dataTransformer = dataTransformer;
            _serializer = serializer;
        }

        public async UniTask<bool> TrySaveAsync<TData>(string key, TData data, CancellationToken cancellationToken)
        {
            try
            {
                var serialized = _serializer.Serialize(data);
                _cache[key] = serialized;

                var transformedData = _dataTransformer.TransformForStorage(serialized);

                await PlatformSaveAsync(key, transformedData, cancellationToken);

#if IS_TEST
                Debug.Log($"[{GetType().Name}::SaveAsync] Saved data for key '{key}'");
#endif
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{GetType().Name}::SaveAsync] Error during saving data: {ex.Message}");
                return false;
            }
        }

        public async UniTask<TData> LoadAsync<TData>(string key, CancellationToken cancellationToken)
        {
            try
            {
                if (_cache.TryGetValue(key, out var cachedData))
                    return _serializer.Deserialize<TData>(cachedData);

                var storedData = await PlatformLoadAsync(key, cancellationToken);
                if (storedData == null)
                    return default;

                var buffer = _dataTransformer.TransformFromStorage(storedData);
                if (buffer == null || buffer.Length == 0)
                    return default;

                _cache[key] = buffer;

                var data = _serializer.Deserialize<TData>(buffer);

#if IS_TEST
                Debug.Log("[{GetType().Name}::LoadAsync] " +
                          "Loaded data for key '{key}' with type '{typeof(TData).Name}' and value '{data}'");
#endif

                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{GetType().Name}::LoadAsync] Error loading data: {ex.Message}");
                return default;
            }
        }

        public async UniTask<bool> HasKeyAsync(string key, CancellationToken cancellationToken)
        {
            if (_cache.ContainsKey(key))
                return true;

            return await PlatformHasKeyAsync(key, cancellationToken);
        }

        public async UniTask<bool> TryDeleteKeyAsync(string key, CancellationToken cancellationToken)
        {
            try
            {
                _cache.Remove(key);
                await PlatformDeleteKeyAsync(key, cancellationToken);

#if IS_TEST
                Debug.Log($"[{GetType().Name}::TryDeleteKeyAsync] Deleted key '{key}'");
#endif

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{GetType().Name}::TryDeleteKeyAsync] Error deleting key: {ex.Message}");
                return false;
            }
        }

        public virtual async UniTask<bool> TryDeleteAllAsync(CancellationToken cancellationToken)
        {
            try
            {
                _cache.Clear();
                var success = await PlatformTryDeleteAllAsync(cancellationToken);

#if IS_TEST
                if (success)
                    Debug.Log($"[{GetType().Name}::TryDeleteAllAsync] Successfully deleted all data");
                else
                    Debug.LogWarning($"[{GetType().Name}::TryDeleteAllAsync] DeleteAll not supported or failed");
#endif

                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{GetType().Name}::TryDeleteAllAsync] Error deleting all data: {ex.Message}");
                return false;
            }
        }

        protected abstract UniTask PlatformSaveAsync(string key, object transformData, CancellationToken cancellationToken);
        protected abstract UniTask<object> PlatformLoadAsync(string key, CancellationToken cancellationToken);
        protected abstract UniTask<bool> PlatformHasKeyAsync(string key, CancellationToken cancellationToken);
        protected abstract UniTask PlatformDeleteKeyAsync(string key, CancellationToken cancellationToken);
        protected abstract UniTask<bool> PlatformTryDeleteAllAsync(CancellationToken cancellationToken);
    }
}