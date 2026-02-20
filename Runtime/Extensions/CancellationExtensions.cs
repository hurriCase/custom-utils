using System.Threading;
using CustomUtils.Runtime.Extensions.Observables;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for operations involving <see cref="CancellationToken"/>
    /// and <see cref="CancellationTokenSource"/>.
    /// </summary>
    [PublicAPI]
    public static class CancellationExtensions
    {
        /// <summary>
        /// Creates a linked <see cref="CancellationTokenSource"/> that combines the specified <see cref="CancellationToken"/>
        /// with a cancellation token associated with the destruction of the specified <see cref="MonoBehaviour"/> instance.
        /// </summary>
        /// <param name="token">The base <see cref="CancellationToken"/> to link with.</param>
        /// <param name="target">The <see cref="MonoBehaviour"/> whose destruction token will be linked.</param>
        /// <returns>A <see cref="CancellationTokenSource"/> linked to both the provided token and the destruction token of the specified <see cref="MonoBehaviour"/>.</returns>
        public static CancellationTokenSource CreateLinkedTokenSourceWithDestroy(this CancellationToken token,
            MonoBehaviour target) =>
            CancellationTokenSource.CreateLinkedTokenSource(
                token,
                target.destroyCancellationToken
            );

        /// <summary>
        /// Cancels and disposes the current <see cref="CancellationTokenSource"/> if it exists,
        /// and creates a new <see cref="CancellationTokenSource"/>.
        /// </summary>
        /// <param name="tokenSource">The <see cref="CancellationTokenSource"/> to renew.
        /// It will be replaced with a new instance.</param>
        public static CancellationToken GetFreshToken(ref CancellationTokenSource tokenSource)
        {
            tokenSource?.Cancel();
            tokenSource?.Dispose();
            tokenSource = new CancellationTokenSource();
            return tokenSource.Token;
        }

        /// <summary>
        /// Returns a CancellationToken that will be cancelled when OnDisable is called on the component's GameObject.
        /// </summary>
        public static CancellationToken GetDisableCancellationToken(this Component component)
        {
            if (!component || !component.gameObject)
                return new CancellationToken(canceled: true);

            return component.GetOrAddComponent<ObservableDisableTrigger>().GetCancellationToken();
        }
    }
}