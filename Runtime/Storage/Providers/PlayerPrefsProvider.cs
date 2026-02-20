using System.Threading;
using CustomUtils.Runtime.Serializer;
using CustomUtils.Runtime.Storage.Base;
using CustomUtils.Runtime.Storage.DataTransformers;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Storage.Providers
{
    /// <summary>
    /// PlayerPrefs provider with TryDeleteAll support
    /// </summary>
    [PublicAPI]
    internal sealed class PlayerPrefsProvider : BaseStorageProvider
    {
        public PlayerPrefsProvider() : base(new StringDataTransformer(), SerializerProvider.Serializer) { }

        protected override UniTask PlatformSaveAsync(string key, object transformData, CancellationToken token)
        {
            if (transformData is not string serializedString)
                return UniTask.CompletedTask;

            PlayerPrefs.SetString(key, serializedString);
            PlayerPrefs.Save();

            return UniTask.CompletedTask;
        }

        protected override UniTask<object> PlatformLoadAsync(string key, CancellationToken token)
            => UniTask.FromResult<object>(PlayerPrefs.GetString(key, null));

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