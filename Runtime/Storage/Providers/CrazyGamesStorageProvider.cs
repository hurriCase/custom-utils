#if CRAZY_GAMES
using System;
using System.Threading;
using CrazyGames;
using CustomUtils.Runtime.Serializer;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Storage.Providers
{
    /// <inheritdoc />
    /// <summary>
    /// Stores data using the CrazyGames SDK. Requires the <c>CRAZY_GAMES</c> scripting define symbol.
    /// </summary>
    [PublicAPI]
    public sealed class CrazyGamesStorageProvider : BaseCloudStorageProvider<string>
    {
        private readonly IStringSerializer _serializer;

        public CrazyGamesStorageProvider(IStringSerializer serializer, TimeSpan debounceDelay) : base(debounceDelay)
        {
            _serializer = serializer;
        }

        protected override string Serialize<TData>(TData data) => _serializer.SerializeToString(data);
        protected override TData Deserialize<TData>(string raw) => _serializer.DeserializeFromString<TData>(raw);

        protected override UniTask PlatformSaveAsync(string key, string data)
        {
            CrazySDK.Data.SetString(key, data);
            return UniTask.CompletedTask;
        }

        protected override UniTask<string> PlatformLoadAsync(string key, CancellationToken token)
            => UniTask.FromResult(CrazySDK.Data.GetString(key, null));

        protected override UniTask<bool> PlatformHasKeyAsync(string key, CancellationToken token)
            => UniTask.FromResult(CrazySDK.Data.HasKey(key));

        protected override UniTask PlatformDeleteKeyAsync(string key, CancellationToken token)
        {
            CrazySDK.Data.DeleteKey(key);
            return UniTask.CompletedTask;
        }

        protected override UniTask<bool> PlatformTryDeleteAllAsync(CancellationToken token)
        {
            CrazySDK.Data.DeleteAll();
            return UniTask.FromResult(true);
        }
    }
}
#endif