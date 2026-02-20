using System;
using R3;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions.Observables
{
    /// <summary>
    /// Provides extension methods for <see cref="Observable{T}"/> Do operator with MonoBehaviour context.
    /// </summary>
    public static partial class ObservableExtensions
    {
        /// <summary>
        /// Performs a side effect with MonoBehaviour instance.
        /// </summary>
        /// <typeparam name="TBehaviour">MonoBehaviour type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <param name="observable">Observable to perform side effect on.</param>
        /// <param name="behaviour">MonoBehaviour instance passed to action.</param>
        /// <param name="onNext">Action called with MonoBehaviour instance.</param>
        public static Observable<T> Do<TBehaviour, T>(
            this Observable<T> observable,
            TBehaviour behaviour,
            Action<TBehaviour> onNext)
            where TBehaviour : MonoBehaviour
        {
            return observable.Do((behaviour, onNext), static (_, tuple) => tuple.onNext(tuple.behaviour));
        }

        /// <summary>
        /// Performs a side effect with observable value and MonoBehaviour instance.
        /// </summary>
        /// <typeparam name="TBehaviour">MonoBehaviour type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <param name="observable">Observable to perform side effect on.</param>
        /// <param name="behaviour">MonoBehaviour instance passed to action.</param>
        /// <param name="onNext">Action called with observable value and MonoBehaviour instance.</param>
        public static Observable<T> Do<TBehaviour, T>(
            this Observable<T> observable,
            TBehaviour behaviour,
            Action<T, TBehaviour> onNext)
            where TBehaviour : MonoBehaviour
        {
            return observable.Do((behaviour, onNext), static (value, tuple) => tuple.onNext(value, tuple.behaviour));
        }

        /// <summary>
        /// Performs a side effect with additional data and MonoBehaviour instance.
        /// </summary>
        /// <typeparam name="TBehaviour">MonoBehaviour type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <typeparam name="TTuple">Additional data type.</typeparam>
        /// <param name="observable">Observable to perform side effect on.</param>
        /// <param name="behaviour">MonoBehaviour instance passed to action.</param>
        /// <param name="tuple">Additional data passed to action.</param>
        /// <param name="onNext">Action called with additional data and MonoBehaviour instance.</param>
        public static Observable<T> Do<TBehaviour, T, TTuple>(
            this Observable<T> observable,
            TBehaviour behaviour,
            TTuple tuple,
            Action<TTuple, TBehaviour> onNext)
            where TBehaviour : MonoBehaviour
        {
            return observable.Do((behaviour, tuple, onNext),
                static (_, state) => state.onNext(state.tuple, state.behaviour));
        }

        /// <summary>
        /// Performs a side effect with observable value, MonoBehaviour instance, and additional data.
        /// </summary>
        /// <typeparam name="TBehaviour">MonoBehaviour type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <typeparam name="TTuple">Additional data type.</typeparam>
        /// <param name="observable">Observable to perform side effect on.</param>
        /// <param name="behaviour">MonoBehaviour instance passed to action.</param>
        /// <param name="tuple">Additional data passed to action.</param>
        /// <param name="onNext">Action called with observable value, MonoBehaviour instance, and additional data.</param>
        public static Observable<T> Do<TBehaviour, T, TTuple>(
            this Observable<T> observable,
            TBehaviour behaviour,
            TTuple tuple,
            Action<T, TBehaviour, TTuple> onNext)
            where TBehaviour : MonoBehaviour
        {
            return observable.Do((behaviour, tuple, onNext),
                static (value, state) => state.onNext(value, state.behaviour, state.tuple));
        }
    }
}