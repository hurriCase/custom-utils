#if CRAZY_GAMES
using System.Threading;
using CustomUtils.Runtime.Storage.Base;
using CustomUtils.Runtime.Storage.DataTransformers;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using CrazyGames;
using CustomUtils.Runtime.Serializer;

namespace CustomUtils.Runtime.Storage.Providers
{
    /// <summary>
    /// PlayerPrefs provider with TryDeleteAll support
    /// </summary>
    [PublicAPI]
    internal sealed class CrazyGamesStorageProvider : BaseStorageProvider
    {
        public CrazyGamesStorageProvider() : base(new StringDataTransformer(), SerializerProvider.Serializer) { }

        protected override UniTask PlatformSaveAsync(string key, object transformData, CancellationToken token)
        {
            if (transformData is not string serializedString)
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