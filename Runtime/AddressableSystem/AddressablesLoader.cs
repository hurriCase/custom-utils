using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace CustomUtils.Runtime.AddressableSystem
{
    [PublicAPI]
    public sealed class AddressablesLoader : IAddressablesLoader
    {
        public async UniTask<T> LoadAsync<T>(AssetReference assetReference, CancellationToken token)
            where T : Object =>
            await LoadAsync<T>(assetReference.AssetGUID, token);

        public async UniTask<T> LoadAsync<T>(string assetGuid, CancellationToken token) where T : Object
        {
            AddressablesLogger.Log(ZString.Format("[PrefabLoader::LoadAsync] Loading {0}...", typeof(T).Name));

#if ADDRESSABLES_LOG_ALL
            using var stopWatchScope = AddressablesLogger.LogWithTimePast("[PrefabLoader::LoadAsync]");
#endif
            var asset = await Addressables.LoadAssetAsync<T>(assetGuid).WithCancellation(token);

            AddressablesLogger.Log($"[PrefabLoader::LoadAsync] Loaded '{asset.name}' ({typeof(T).Name})");
            return asset;
        }

        public async UniTask<TComponent> LoadComponentAsync<TComponent>(
            AssetReference assetReference,
            CancellationToken token)
            where TComponent : Component
        {
            AddressablesLogger.Log(ZString.Format("[PrefabLoader::LoadAsync] Loading {0}...", typeof(TComponent).Name));

#if ADDRESSABLES_LOG_ALL
            using var stopWatchScope = AddressablesLogger.LogWithTimePast("[PrefabLoader::LoadAsync]");
#endif
            var asset =
                await Addressables.LoadAssetAsync<GameObject>(assetReference).WithCancellation(token);

            AddressablesLogger.Log(ZString.Format("[PrefabLoader::LoadAsync] Loaded '{0}' ({0})",
                asset.name,
                typeof(TComponent).Name));

            return asset.GetComponent<TComponent>();
        }

        public async UniTask AssignImageAsync(Image image, AssetReference assetReference, CancellationToken token)
            => image.sprite = await LoadAsync<Sprite>(assetReference, token);

        public async UniTask AssignImageAsync(Image image, CachedSprite cachedSprite, CancellationToken token)
        {
            if (cachedSprite.IsValid)
                image.sprite = await LoadAsync<Sprite>(cachedSprite.AssetGUID, token);
        }
    }
}