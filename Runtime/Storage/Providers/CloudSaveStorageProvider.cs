#if CLOUD_SAVE_INSTALLED
using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Serializer;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.Services.CloudSave;
using UnityEngine.Pool;
using DeleteAllOptions = Unity.Services.CloudSave.Models.Data.Player.DeleteAllOptions;
using DeleteOptions = Unity.Services.CloudSave.Models.Data.Player.DeleteOptions;
using LoadOptions = Unity.Services.CloudSave.Models.Data.Player.LoadOptions;
using SaveOptions = Unity.Services.CloudSave.Models.Data.Player.SaveOptions;

namespace CustomUtils.Runtime.Storage.Providers
{
    /// <inheritdoc />
    /// <summary>
    /// Stores data using Unity Cloud Save. Requires Unity Gaming Services to be initialized before use.
    /// </summary>
    [PublicAPI]
    public sealed class CloudSaveStorageProvider : CloudStorageProviderBase<string>
    {
        private readonly IStringSerializer _serializer;

        public CloudSaveStorageProvider(IStringSerializer serializer, TimeSpan debounceDelay) : base(debounceDelay)
        {
            _serializer = serializer;
        }

        protected override string Serialize<TData>(TData data) => _serializer.SerializeToString(data);
        protected override TData Deserialize<TData>(string raw) => _serializer.DeserializeFromString<TData>(raw);

        protected override async UniTask PlatformSaveAsync(string key, string data)
        {
            var saveData = DictionaryPool<string, object>.Get();

            try
            {
                saveData[key] = data;
                await CloudSaveService.Instance.Data.Player.SaveAsync(saveData, new SaveOptions());
            }
            finally
            {
                DictionaryPool<string, object>.Release(saveData);
            }
        }

        protected override async UniTask<string> PlatformLoadAsync(string key, CancellationToken token)
        {
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { key },
                new LoadOptions());

            return result.TryGetValue(key, out var item) ? item.Value.GetAsString() : null;
        }

        protected override async UniTask<bool> PlatformHasKeyAsync(string key, CancellationToken token)
        {
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { key },
                new LoadOptions());

            return result.ContainsKey(key);
        }

        protected override async UniTask PlatformDeleteKeyAsync(string key, CancellationToken token)
            => await CloudSaveService.Instance.Data.Player.DeleteAsync(key, new DeleteOptions());

        protected override async UniTask<bool> PlatformTryDeleteAllAsync(CancellationToken token)
        {
            await CloudSaveService.Instance.Data.Player.DeleteAllAsync(new DeleteAllOptions());
            return true;
        }
    }
}
#endif