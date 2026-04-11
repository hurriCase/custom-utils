using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Serializer;
using Cysharp.Threading.Tasks;

namespace CustomUtils.Runtime.Storage.Base
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for storage providers. Handles serialization, caching, and error logging.
    /// Extend this to implement a custom storage backend.
    /// </summary>
    public abstract class BaseStorageProvider : IStorageProvider
    {
        private readonly Dictionary<string, byte[]> _cache = new();

        private readonly IDataTransformer _dataTransformer;

        protected BaseStorageProvider(IDataTransformer dataTransformer)
        {
            _dataTransformer = dataTransformer;
        }

        public virtual async UniTask<bool> TrySaveAsync<TData>(string key, TData data)
        {
            try
            {
                var serialized = SerializerProvider.Serializer.Serialize(data);
                _cache[key] = serialized;

                var transformedData = _dataTransformer.TransformForStorage(serialized);

                await PlatformSaveAsync(key, transformedData);

                Logger.Log($"[{GetType().Name}::SaveAsync] Saved data for key '{key}'");
                return true;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.LogError($"[{GetType().Name}::SaveAsync] Error during saving data: {e.Message}");
                return false;
            }
        }

        public async UniTask<TData> LoadAsync<TData>(string key, CancellationToken token)
        {
            try
            {
                if (_cache.TryGetValue(key, out var cachedData))
                    return SerializerProvider.Serializer.Deserialize<TData>(cachedData);

                var storedData = await PlatformLoadAsync(key, token);
                if (storedData == null)
                    return default;

                var buffer = _dataTransformer.TransformFromStorage(storedData);
                if (buffer == null || buffer.Length == 0)
                    return default;

                _cache[key] = buffer;

                var data = SerializerProvider.Serializer.Deserialize<TData>(buffer);

                Logger.Log($"[{GetType().Name}::LoadAsync] " +
                           $"Loaded data for key '{key}' with type '{typeof(TData).Name}' and value '{data}'");

                return data;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.LogError($"[{GetType().Name}::LoadAsync] Error loading data: {e.Message}");
                return default;
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
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.LogError($"[{GetType().Name}::TryDeleteKeyAsync] Error deleting key: {e.Message}");
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
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.LogError($"[{GetType().Name}::TryDeleteAllAsync] Error deleting all data: {e.Message}");
                return false;
            }
        }

        protected bool TryGetTransformedData<TTransformed>(object transformData, out TTransformed result)
        {
            if (transformData is TTransformed casted)
            {
                result = casted;
                return true;
            }

            Logger.LogError($"[{GetType().Name}::TryGetTransformedData] " +
                            $"Unexpected data type: {transformData?.GetType().Name ?? "null"}");
            result = default;
            return false;
        }

        protected abstract UniTask PlatformSaveAsync(string key, object transformData);
        protected abstract UniTask<object> PlatformLoadAsync(string key, CancellationToken token);
        protected abstract UniTask<bool> PlatformHasKeyAsync(string key, CancellationToken token);
        protected abstract UniTask PlatformDeleteKeyAsync(string key, CancellationToken token);
        protected abstract UniTask<bool> PlatformTryDeleteAllAsync(CancellationToken token);
    }
}