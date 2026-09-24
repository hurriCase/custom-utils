using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace CustomUtils.Runtime.AddressableSystem
{
    /// <summary>
    /// Provides extension methods that bind loaded Addressable assets to a lifetime.
    /// </summary>
    [PublicAPI]
    public static class AddressableReleaseExtensions
    {
        /// <typeparam name="T">Type of the loaded asset.</typeparam>
        /// <param name="loadTask">Load task returned by <see cref="IAddressablesLoader"/>.</param>
        /// <param name="lifetimeToken">
        /// Token that defines how long the asset lives, e.g. <c>destroyCancellationToken</c>.
        /// Must eventually be cancelled; <see cref="CancellationToken.None"/> never releases the asset.
        /// </param>
        /// <returns>The loaded asset, valid until <paramref name="lifetimeToken"/> is cancelled.</returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown if <paramref name="lifetimeToken"/> was cancelled before loading finished; the asset is released.
        /// </exception>
        public static async UniTask<T> ReleaseOn<T>(this UniTask<T> loadTask, CancellationToken lifetimeToken)
            where T : Object
        {
            var asset = await loadTask;

            if (lifetimeToken.IsCancellationRequested)
            {
                Addressables.Release(asset);
                throw new OperationCanceledException(lifetimeToken);
            }

            lifetimeToken.Register(static state => Addressables.Release(state), asset);
            return asset;
        }
    }
}