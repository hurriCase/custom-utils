using System;
using CustomUtils.Runtime.Storage;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="PersistentReactiveProperty{T}"/>.
    /// </summary>
    [PublicAPI]
    public static class PersistentReactivePropertyExtensions
    {
        /// <summary>
        /// Subscribes to persistent reactive property and automatically disposes on MonoBehaviour destruction.
        /// </summary>
        /// <typeparam name="TSelf">MonoBehaviour type.</typeparam>
        /// <typeparam name="T">Property value type.</typeparam>
        /// <param name="observable">PersistentReactiveProperty to subscribe to.</param>
        /// <param name="self">MonoBehaviour instance for disposal registration.</param>
        /// <param name="onNext">Action called with MonoBehaviour instance.</param>
        public static void SubscribeUntilDestroy<TSelf, T>(
            this PersistentReactiveProperty<T> observable,
            TSelf self,
            Action<TSelf> onNext)
            where TSelf : MonoBehaviour
        {
            observable.Subscribe((self, onNext), static (_, tuple) => tuple.onNext(tuple.self))
                .RegisterTo(self.GetCancellationTokenOnDestroy());
        }

        /// <summary>
        /// Subscribes to persistent reactive property and automatically disposes on MonoBehaviour destruction.
        /// </summary>
        /// <typeparam name="TSelf">MonoBehaviour type.</typeparam>
        /// <typeparam name="T">Property value type.</typeparam>
        /// <param name="observable">PersistentReactiveProperty to subscribe to.</param>
        /// <param name="self">MonoBehaviour instance for disposal registration.</param>
        /// <param name="onNext">Action called with property value and MonoBehaviour instance.</param>
        public static void SubscribeUntilDestroy<TSelf, T>(
            this PersistentReactiveProperty<T> observable,
            TSelf self,
            Action<T, TSelf> onNext)
            where TSelf : MonoBehaviour
        {
            observable.Subscribe((self, onNext), static (value, tuple) => tuple.onNext(value, tuple.self))
                .RegisterTo(self.GetCancellationTokenOnDestroy());
        }
    }
}