using System.Threading;
using CustomUtils.Runtime.Serializer;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Storage.Providers
{
    /// <inheritdoc />
    /// <summary>
    /// Stores data using Unity's <see cref="T:UnityEngine.PlayerPrefs">UnityEngine.PlayerPrefs</see>. Suitable for editor and mobile platforms.
    /// </summary>
    [PublicAPI]
    public sealed class PlayerPrefsProvider : BaseStorageProvider<string>
    {
        private readonly IStringSerializer _serializer;

        public PlayerPrefsProvider(IStringSerializer serializer)
        {
            _serializer = serializer;
        }

        protected override string Serialize<TData>(TData data) => _serializer.SerializeToString(data);
        protected override TData Deserialize<TData>(string raw) => _serializer.DeserializeFromString<TData>(raw);

        protected override UniTask PlatformSaveAsync(string key, string data)
        {
            PlayerPrefs.SetString(key, data);
            PlayerPrefs.Save();
            return UniTask.CompletedTask;
        }

        protected override UniTask<string> PlatformLoadAsync(string key, CancellationToken token)
            => UniTask.FromResult(PlayerPrefs.GetString(key, null));

        protected override UniTask<bool> PlatformHasKeyAsync(string key, CancellationToken token)
            => UniTask.FromResult(PlayerPrefs.HasKey(key));

        protected override UniTask PlatformDeleteKeyAsync(string key, CancellationToken token)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            return UniTask.CompletedTask;
        }

        protected override UniTask<bool> PlatformTryDeleteAllAsync(CancellationToken token)
        {
            PlayerPrefs.DeleteAll();
            return UniTask.FromResult(true);
        }
    }
}