using System;
using JetBrains.Annotations;
using R3;

namespace CustomUtils.Runtime.Extensions.Observables
{
    /// <summary>
    /// Provides extension methods for <see cref="Observable{T}"/> with automatic disposal on destruction.
    /// </summary>
    public static partial class ObservableExtensions
    {
        /// <summary>
        /// Subscribes to observable with automatic closure prevention.
        /// </summary>
        /// <typeparam name="TSelf">Instance type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <param name="observable">Observable to subscribe to.</param>
        /// <param name="behaviour">Instance for callback.</param>
        /// <param name="onNext">Action called with instance.</param>
        /// <returns>Disposable subscription.</returns>
        public static IDisposable SubscribeSelf<TSelf, T>(
            this Observable<T> observable,
            TSelf behaviour,
            Action<TSelf> onNext) =>
            observable.Subscribe((behaviour, onNext),
                static (_, tuple) => tuple.onNext(tuple.behaviour));

        /// <summary>
        /// Subscribes to observable with automatic closure prevention.
        /// </summary>
        /// <typeparam name="TSelf">Instance type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <param name="observable">Observable to subscribe to.</param>
        /// <param name="behaviour">Instance for callback.</param>
        /// <param name="onNext">Action called with observable value and instance.</param>
        public static void SubscribeSelf<TSelf, T>(
            this Observable<T> observable,
            TSelf behaviour,
            Action<T, TSelf> onNext) =>
            observable.Subscribe((behaviour, onNext),
                static (value, tuple) => tuple.onNext(value, tuple.behaviour));

        /// <summary>
        /// Subscribes to observable with automatic closure prevention.
        /// </summary>
        /// <typeparam name="TSelf">Instance type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <typeparam name="TTuple">Additional data type.</typeparam>
        /// <param name="observable">Observable to subscribe to.</param>
        /// <param name="behaviour">Instance for callback.</param>
        /// <param name="tuple">Additional data passed to the action.</param>
        /// <param name="onNext">Action called with additional data and instance.</param>
        public static void SubscribeSelf<TSelf, T, TTuple>(
            this Observable<T> observable,
            TSelf behaviour,
            TTuple tuple,
            Action<TTuple, TSelf> onNext) =>
            observable.Subscribe((behaviour, tuple, onNext),
                static (_, state) => state.onNext(state.tuple, state.behaviour));

        /// <summary>
        /// Subscribes to observable with automatic closure prevention.
        /// </summary>
        /// <typeparam name="TSelf">Instance type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <typeparam name="TTuple">Additional data type.</typeparam>
        /// <param name="observable">Observable to subscribe to.</param>
        /// <param name="behaviour">Instance for callback.</param>
        /// <param name="tuple">Additional data passed to the action.</param>
        /// <param name="onNext">Action called with observable value, instance, and additional data.</param>
        public static void SubscribeSelf<TSelf, T, TTuple>(
            this Observable<T> observable,
            TSelf behaviour,
            TTuple tuple,
            Action<T, TSelf, TTuple> onNext) =>
            observable.Subscribe((behaviour, tuple, onNext),
                static (value, state) => state.onNext(value, state.behaviour, state.tuple));
    }
}