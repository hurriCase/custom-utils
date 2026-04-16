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
    /// <inheritdoc />
    /// <summary>
    /// Stores data using Unity Cloud Save. Requires Unity Gaming Services to be initialized before use.
    /// </summary>
    [PublicAPI]
    public sealed class CloudSaveStorageProvider : BaseCloudStorageProvider<string>
    {
        private readonly IStringSerializer _serializer;
        private readonly Dictionary<string, object> _saveBuffer = new(capacity: 1);

        public CloudSaveStorageProvider(IStringSerializer serializer, TimeSpan debounceDelay) : base(debounceDelay)
        {
            _serializer = serializer;
        }

        protected override string Serialize<TData>(TData data) => _serializer.SerializeToString(data);
        protected override TData Deserialize<TData>(string raw) => _serializer.DeserializeFromString<TData>(raw);

        protected override async UniTask PlatformSaveAsync(string key, string data)
        {
            _saveBuffer[key] = data;
            await CloudSaveService.Instance.Data.Player.SaveAsync(_saveBuffer);
            _saveBuffer.Clear();
        }

        protected override async UniTask<string> PlatformLoadAsync(string key, CancellationToken token)
        {
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { key });

            return result.TryGetValue(key, out var item) ? item.Value.GetAsString() : null;
        }

        protected override async UniTask<bool> PlatformHasKeyAsync(string key, CancellationToken token)
        {
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { key });

            return result.ContainsKey(key);
        }

        protected override async UniTask PlatformDeleteKeyAsync(string key, CancellationToken token)
            => await CloudSaveService.Instance.Data.Player.DeleteAsync(key, new DeleteOptions());

        protected override async UniTask<bool> PlatformTryDeleteAllAsync(CancellationToken token)
        {
            var keys = await CloudSaveService.Instance.Data.Player.ListAllKeysAsync();
            foreach (var keyItem in keys)
                await CloudSaveService.Instance.Data.Player.DeleteAsync(keyItem.Key, new DeleteOptions());

            return true;
        }
    }
}
#endif