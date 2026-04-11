#if CRAZY_GAMES
using System;
using System.Threading;
using CustomUtils.Runtime.Storage.Base;
using CustomUtils.Runtime.Storage.DataTransformers;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using CrazyGames;

namespace CustomUtils.Runtime.Storage.Providers
{
    /// <inheritdoc />
    /// <summary>
    /// Stores data using the CrazyGames SDK. Requires the <c>CRAZY_GAMES</c> scripting define symbol.
    /// </summary>
    [PublicAPI]
    public sealed class CrazyGamesStorageProvider : BaseCloudStorageProvider
    {
        public CrazyGamesStorageProvider(TimeSpan debounceDelay) : base(new StringDataTransformer(), debounceDelay) { }

        protected override UniTask PlatformSaveAsync(string key, object transformData)
        {
            if (!TryGetTransformedData<string>(transformData, out var serializedString))
                return UniTask.CompletedTask;

            CrazySDK.Data.SetString(key, serializedString);

            return UniTask.CompletedTask;
        }

        protected override UniTask<object> PlatformLoadAsync(string key, CancellationToken token)
            => UniTask.FromResult<object>(CrazySDK.Data.GetString(key, null));

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