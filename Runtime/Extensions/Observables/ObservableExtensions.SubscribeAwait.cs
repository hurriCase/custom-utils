using System;
using System.Threading;
using System.Threading.Tasks;
using R3;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions.Observables
{
    /// <summary>
    /// Provides extension methods for <see cref="Observable{T}"/> with exclusive async handling,
    /// which ignores incoming values while an async operation is already in progress.
    /// </summary>
    public static partial class ObservableExtensions
    {
        /// <summary>
        /// Subscribes to observable with async handler using <see cref="AwaitOperation.Drop"/> strategy.
        /// Incoming values are ignored while an async operation is already in progress.
        /// Automatically disposes on MonoBehaviour destruction.
        /// </summary>
        /// <typeparam name="TSelf">MonoBehaviour type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <param name="observable">Observable to subscribe to.</param>
        /// <param name="self">MonoBehaviour instance for disposal registration.</param>
        /// <param name="onNextAsync">Async action called with MonoBehaviour instance.</param>
        public static IDisposable SubscribeAwaitExclusive<TSelf, T>(
            this Observable<T> observable,
            TSelf self,
            Func<TSelf, CancellationToken, ValueTask> onNextAsync)
            where TSelf : MonoBehaviour
        {
            var subscription = observable.SubscribeAwait((self, onNextAsync),
                static (_, state, cancellationToken) =>
                    state.onNextAsync(state.self, cancellationToken), AwaitOperation.Drop);
            subscription.RegisterTo(self.destroyCancellationToken);
            return subscription;
        }

        /// <summary>
        /// Subscribes to observable with async handler using <see cref="AwaitOperation.Drop"/> strategy.
        /// Incoming values are ignored while an async operation is already in progress.
        /// Automatically disposes on MonoBehaviour destruction.
        /// </summary>
        /// <typeparam name="TSelf">MonoBehaviour type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <param name="observable">Observable to subscribe to.</param>
        /// <param name="self">MonoBehaviour instance for disposal registration.</param>
        /// <param name="onNextAsync">Async action called with observable value and MonoBehaviour instance.</param>
        public static IDisposable SubscribeAwaitExclusive<TSelf, T>(
            this Observable<T> observable,
            TSelf self,
            Func<T, TSelf, CancellationToken, ValueTask> onNextAsync)
            where TSelf : MonoBehaviour
        {
            var subscription = observable.SubscribeAwait((self, onNextAsync),
                static (value, state, cancellationToken) =>
                    state.onNextAsync(value, state.self, cancellationToken), AwaitOperation.Drop);
            subscription.RegisterTo(self.destroyCancellationToken);
            return subscription;
        }

        /// <summary>
        /// Subscribes to observable with async handler using <see cref="AwaitOperation.Drop"/> strategy.
        /// Incoming values are ignored while an async operation is already in progress.
        /// Automatically disposes on MonoBehaviour destruction.
        /// </summary>
        /// <typeparam name="TSelf">MonoBehaviour type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <typeparam name="TTuple">Additional data type.</typeparam>
        /// <param name="observable">Observable to subscribe to.</param>
        /// <param name="self">MonoBehaviour instance for disposal registration.</param>
        /// <param name="tuple">Additional data passed to the async action.</param>
        /// <param name="onNextAsync">Async action called with additional data and MonoBehaviour instance.</param>
        public static IDisposable SubscribeAwaitExclusive<TSelf, T, TTuple>(
            this Observable<T> observable,
            TSelf self,
            TTuple tuple,
            Func<TTuple, TSelf, CancellationToken, ValueTask> onNextAsync)
            where TSelf : MonoBehaviour
        {
            var subscription = observable.SubscribeAwait((self, tuple, onNextAsync),
                static (_, state, cancellationToken) =>
                    state.onNextAsync(state.tuple, state.self, cancellationToken), AwaitOperation.Drop);
            subscription.RegisterTo(self.destroyCancellationToken);
            return subscription;
        }

        /// <summary>
        /// Subscribes to observable with async handler using <see cref="AwaitOperation.Drop"/> strategy.
        /// Incoming values are ignored while an async operation is already in progress.
        /// Automatically disposes on MonoBehaviour destruction.
        /// </summary>
        /// <typeparam name="TSelf">MonoBehaviour type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <typeparam name="TTuple">Additional data type.</typeparam>
        /// <param name="observable">Observable to subscribe to.</param>
        /// <param name="self">MonoBehaviour instance for disposal registration.</param>
        /// <param name="tuple">Additional data passed to the async action.</param>
        /// <param name="onNextAsync">Async action called with observable value, MonoBehaviour instance, and additional data.</param>
        public static IDisposable SubscribeAwaitExclusive<TSelf, T, TTuple>(
            this Observable<T> observable,
            TSelf self,
            TTuple tuple,
            Func<T, TSelf, TTuple, CancellationToken, ValueTask> onNextAsync)
            where TSelf : MonoBehaviour
        {
            var subscription = observable.SubscribeAwait((self, tuple, onNextAsync),
                static (value, state, cancellationToken) =>
                    state.onNextAsync(value, state.self, state.tuple, cancellationToken), AwaitOperation.Drop);
            subscription.RegisterTo(self.destroyCancellationToken);
            return subscription;
        }
    }
}