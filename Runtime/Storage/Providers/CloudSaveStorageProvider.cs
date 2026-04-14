#if CLOUD_SAVE_INSTALLED
using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Serializer;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.Services.CloudSave;
using DeleteOptions = Unity.Services.CloudSave.Models.Data.Player.DeleteOptions;

namespace CustomUtils.Runtime.Storage.Providers
{
    [PublicAPI]
    public sealed class CloudSaveStorageProvider : BaseCloudStorageProvider
    {
        private readonly IStringSerializer _stringSerializer;

        public CloudSaveStorageProvider(IStringSerializer stringSerializer, TimeSpan debounceDelay) : base(
            debounceDelay)
        {
            _stringSerializer = stringSerializer;
        }

        private readonly Dictionary<string, object> _saveBuffer = new(capacity: 1);

        private readonly Dictionary<string, string> _cache = new();

        protected override async UniTask<bool> OnTrySaveAsync<TData>(string key, TData data)
        {
            try
            {
                var serialized = _stringSerializer.SerializeToString(data);
                _cache[key] = serialized;

                _saveBuffer[key] = serialized;
                await CloudSaveService.Instance.Data.Player.SaveAsync(_saveBuffer);
                _saveBuffer.Clear();

                Logger.Log($"[{nameof(CloudSaveStorageProvider)}::SaveAsync] Saved data for key '{key}'");
                return true;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.LogError($"[{nameof(CloudSaveStorageProvider)}::SaveAsync] Error during saving data: {e.Message}");
                return false;
            }
        }

        public override async UniTask<TData> LoadAsync<TData>(string key, CancellationToken token)
        {
            try
            {
                var rawData = await GetRawDataAsync(key, token);
                var data = _stringSerializer.DeserializeFromString<TData>(rawData);

                Logger.Log($"[{nameof(CloudSaveStorageProvider)}::LoadAsync] " +
                           $"Loaded data for key '{key}' with type '{typeof(TData).Name}' and value '{data}'");

                return data;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.LogError($"[{nameof(CloudSaveStorageProvider)}::LoadAsync] Error loading data: {e.Message}");
                return default;
            }
        }

        public override async UniTask<bool> HasKeyAsync(string key, CancellationToken token)
        {
            if (_cache.ContainsKey(key))
                return true;

            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { key });

            return result.ContainsKey(key);
        }

        public override async UniTask<bool> TryDeleteKeyAsync(string key, CancellationToken token)
        {
            try
            {
                _cache.Remove(key);
                await CloudSaveService.Instance.Data.Player.DeleteAsync(key, new DeleteOptions());

                Logger.Log($"[{nameof(CloudSaveStorageProvider)}::TryDeleteKeyAsync] Deleted key '{key}'");

                return true;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.LogError($"[{nameof(CloudSaveStorageProvider)}::TryDeleteKeyAsync] Error deleting key: {e.Message}");
                return false;
            }
        }

        public override async UniTask<bool> TryDeleteAllAsync(CancellationToken token)
        {
            try
            {
                _cache.Clear();
                var keys = await CloudSaveService.Instance.Data.Player.ListAllKeysAsync();
                foreach (var keyItem in keys)
                    await CloudSaveService.Instance.Data.Player.DeleteAsync(keyItem.Key, new DeleteOptions());

                Logger.Log($"[{nameof(CloudSaveStorageProvider)}::TryDeleteAllAsync] Successfully deleted all data");

                return true;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.LogError($"[{nameof(CloudSaveStorageProvider)}::TryDeleteAllAsync] Error deleting all data: {e.Message}");
                return false;
            }
        }

        private async UniTask<string> GetRawDataAsync(string key, CancellationToken token)
        {
            if (_cache.TryGetValue(key, out var cachedData))
                return cachedData;

            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { key });

            if (!result.TryGetValue(key, out var item))
                return null;

            var deserialized = item.Value.GetAsString();
            _cache[key] = deserialized;
            return deserialized;
        }
    }
}
#endif