using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Formatter;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace CustomUtils.Runtime.AddressableSystem
{
    [PublicAPI]
    [Preserve]
    public sealed class AddressablesLoader : IAddressablesLoader
    {
        // Addressables.Release(Object) misses assets that completed synchronously (Asset Database play mode)
        // until the end of the frame, so assets are released through their own handles instead
        private readonly Dictionary<Object, Stack<AsyncOperationHandle>> _handles = new();

        public async UniTask<T> LoadAsync<T>(AssetReference assetReference, CancellationToken token)
            where T : Object =>
            await LoadAsync<T>(assetReference.AssetGUID, token);

        public async UniTask<T> LoadAsync<T>(string assetGuid, CancellationToken token) where T : Object
        {
            AddressablesLogger.Log(StringFormatter.Format("[PrefabLoader::LoadAsync] Loading {0}...", typeof(T).Name));

#if ADDRESSABLES_LOG_ALL
            using var stopWatchScope = AddressablesLogger.LogWithTimePast("[PrefabLoader::LoadAsync]");
#endif
            var handle = Addressables.LoadAssetAsync<T>(assetGuid);
            var asset = await handle.WithCancellation(token, autoReleaseWhenCanceled: true);
            TrackHandle(asset, handle);

            AddressablesLogger.Log($"[PrefabLoader::LoadAsync] Loaded '{asset.name}' ({typeof(T).Name})");
            return asset;
        }

        public async UniTask<TComponent> LoadComponentAsync<TComponent>(
            AssetReference assetReference,
            CancellationToken token)
            where TComponent : Component
        {
            AddressablesLogger.Log(StringFormatter.Format("[PrefabLoader::LoadAsync] " +
                                                          "Loading {0}...", typeof(TComponent).Name));

#if ADDRESSABLES_LOG_ALL
            using var stopWatchScope = AddressablesLogger.LogWithTimePast("[PrefabLoader::LoadAsync]");
#endif
            var handle = Addressables.LoadAssetAsync<GameObject>(assetReference);
            var asset = await handle.WithCancellation(token, autoReleaseWhenCanceled: true);
            TrackHandle(asset, handle);

            AddressablesLogger.Log(StringFormatter.Format("[PrefabLoader::LoadAsync] Loaded '{0}' ({0})",
                asset.name,
                typeof(TComponent).Name));

            return asset.GetComponent<TComponent>();
        }

        public async UniTask<Sprite> AssignImageAsync(
            Image image,
            AssetReference assetReference,
            CancellationToken token)
            => image.sprite = await LoadAsync<Sprite>(assetReference, token);

        public async UniTask<Sprite> AssignImageAsync(Image image, CachedSprite cachedSprite, CancellationToken token)
        {
            if (!cachedSprite.IsValid)
                return null;

            return image.sprite = await LoadAsync<Sprite>(cachedSprite.AssetGUID, token);
        }

        public void Release(Object asset)
        {
            if (!asset)
                return;

            if (!_handles.TryGetValue(asset, out var handles))
            {
                Addressables.Release(asset);
                return;
            }

            var handle = handles.Pop();
            if (handles.Count == 0)
                _handles.Remove(asset);

            Addressables.Release(handle);
        }

        private void TrackHandle(Object asset, AsyncOperationHandle handle)
        {
            if (!_handles.TryGetValue(asset, out var handles))
            {
                handles = new Stack<AsyncOperationHandle>();
                _handles.Add(asset, handles);
            }

            handles.Push(handle);
        }
    }
}