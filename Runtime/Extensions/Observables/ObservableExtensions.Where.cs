using System;
using R3;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions.Observables
{
    /// <summary>
    /// Provides extension methods for <see cref="Observable{T}"/> filtering with MonoBehaviour context.
    /// </summary>
    public static partial class ObservableExtensions
    {
        /// <summary>
        /// Filters observable based on predicate that only uses the MonoBehaviour instance.
        /// </summary>
        /// <typeparam name="TSelf">MonoBehaviour type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <param name="observable">Observable to filter.</param>
        /// <param name="self">MonoBehaviour instance for predicate.</param>
        /// <param name="predicate">Predicate function that takes only MonoBehaviour instance.</param>
        /// <returns>Filtered observable.</returns>
        public static Observable<T> Where<TSelf, T>(
            this Observable<T> observable,
            TSelf self,
            Func<TSelf, bool> predicate)
            where TSelf : MonoBehaviour
        {
            return observable.Where((self, predicate), static (_, tuple) => tuple.predicate(tuple.self));
        }

        /// <summary>
        /// Filters observable based on predicate using additional data and MonoBehaviour instance.
        /// </summary>
        /// <typeparam name="TSelf">MonoBehaviour type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <typeparam name="TTuple">Additional data type.</typeparam>
        /// <param name="observable">Observable to filter.</param>
        /// <param name="self">MonoBehaviour instance for predicate.</param>
        /// <param name="tuple">Additional data passed to the predicate.</param>
        /// <param name="predicate">Predicate function that takes additional data and MonoBehaviour instance.</param>
        /// <returns>Filtered observable.</returns>
        public static Observable<T> Where<TSelf, T, TTuple>(
            this Observable<T> observable,
            TSelf self,
            TTuple tuple,
            Func<TTuple, TSelf, bool> predicate)
            where TSelf : MonoBehaviour
        {
            return observable.Where((self, tuple, predicate),
                static (_, state) => state.predicate(state.tuple, state.self));
        }

        /// <summary>
        /// Filters observable based on predicate using observable value, MonoBehaviour instance, and additional data.
        /// </summary>
        /// <typeparam name="TSelf">MonoBehaviour type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <typeparam name="TTuple">Additional data type.</typeparam>
        /// <param name="observable">Observable to filter.</param>
        /// <param name="self">MonoBehaviour instance for predicate.</param>
        /// <param name="tuple">Additional data passed to the predicate.</param>
        /// <param name="predicate">Predicate function that takes observable value, MonoBehaviour instance, and additional data.</param>
        /// <returns>Filtered observable.</returns>
        public static Observable<T> Where<TSelf, T, TTuple>(
            this Observable<T> observable,
            TSelf self,
            TTuple tuple,
            Func<T, TSelf, TTuple, bool> predicate)
            where TSelf : MonoBehaviour
        {
            return observable.Where((self, tuple, predicate),
                static (value, state) => state.predicate(value, state.self, state.tuple));
        }
    }
}