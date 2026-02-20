using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace CustomUtils.Runtime.AddressableSystem
{
    /// <summary>
    /// Interface for loading Addressable assets asynchronously.
    /// </summary>
    [PublicAPI]
    public interface IAddressablesLoader
    {
        /// <summary>
        /// Loads an Addressable asset by reference.
        /// </summary>
        /// <typeparam name="T">Asset type to load.</typeparam>
        /// <param name="assetReference">Asset reference to load.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Loaded asset instance.</returns>
        UniTask<T> LoadAsync<T>(AssetReference assetReference, CancellationToken token)
            where T : Object;

        /// <summary>
        /// Loads an Addressable asset by GUID.
        /// </summary>
        /// <typeparam name="T">Asset type to load.</typeparam>
        /// <param name="assetGuid">Asset GUID to load.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Loaded asset instance.</returns>
        UniTask<T> LoadAsync<T>(string assetGuid, CancellationToken token)
            where T : Object;

        /// <summary>
        /// Loads a GameObject and returns specified component.
        /// </summary>
        /// <typeparam name="TComponent">Component type to retrieve.</typeparam>
        /// <param name="assetReference">GameObject asset reference.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Component from loaded GameObject.</returns>
        UniTask<TComponent> LoadComponentAsync<TComponent>(
            AssetReference assetReference,
            CancellationToken token)
            where TComponent : Component;

        /// <summary>
        /// Loads sprite and assigns to Image component.
        /// </summary>
        /// <param name="image">Target Image component.</param>
        /// <param name="assetReference">Sprite asset reference.</param>
        /// <param name="token">Cancellation token.</param>
        UniTask AssignImageAsync(Image image, AssetReference assetReference, CancellationToken token);

        /// <summary>
        /// Loads sprite from cached sprite data and assigns to Image component.
        /// </summary>
        /// <param name="image">Target Image component.</param>
        /// <param name="cachedSprite">Cached sprite data.</param>
        /// <param name="token">Cancellation token.</param>
        UniTask AssignImageAsync(Image image, CachedSprite cachedSprite, CancellationToken token);
    }
}