using System;
using JetBrains.Annotations;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.Extensions.Observables
{
    /// <summary>
    /// Provides extension methods for <see cref="Observable{T}"/> with automatic disposal on destruction.
    /// </summary>
    [PublicAPI]
    public static partial class ObservableExtensions
    {
        /// <summary>
        /// Subscribes to observable and automatically disposes on MonoBehaviour destruction.
        /// </summary>
        /// <typeparam name="TBehaviour">MonoBehaviour type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <param name="observable">Observable to subscribe to.</param>
        /// <param name="behaviour">MonoBehaviour instance for disposal registration.</param>
        /// <param name="onNext">Action called with MonoBehaviour instance.</param>
        public static void SubscribeUntilDestroy<TBehaviour, T>(
            this Observable<T> observable,
            TBehaviour behaviour,
            Action<TBehaviour> onNext)
            where TBehaviour : MonoBehaviour
        {
            observable.Subscribe((behaviour, onNext), static (_, tuple) => tuple.onNext(tuple.behaviour))
                .RegisterTo(behaviour.destroyCancellationToken);
        }

        /// <summary>
        /// Subscribes to observable and automatically disposes on MonoBehaviour destruction.
        /// </summary>
        /// <typeparam name="TBehaviour">MonoBehaviour type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <param name="observable">Observable to subscribe to.</param>
        /// <param name="behaviour">MonoBehaviour instance for disposal registration.</param>
        /// <param name="onNext">Action called with observable value and MonoBehaviour instance.</param>
        public static void SubscribeUntilDestroy<TBehaviour, T>(
            this Observable<T> observable,
            TBehaviour behaviour,
            Action<T, TBehaviour> onNext)
            where TBehaviour : MonoBehaviour
        {
            observable.Subscribe((behaviour, onNext), static (value, tuple) => tuple.onNext(value, tuple.behaviour))
                .RegisterTo(behaviour.destroyCancellationToken);
        }

        /// <summary>
        /// Subscribes to observable and automatically disposes on MonoBehaviour destruction.
        /// </summary>
        /// <typeparam name="TBehaviour">MonoBehaviour type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <typeparam name="TTuple">Additional data type.</typeparam>
        /// <param name="observable">Observable to subscribe to.</param>
        /// <param name="behaviour">MonoBehaviour instance for disposal registration.</param>
        /// <param name="tuple">Additional data passed to the action.</param>
        /// <param name="onNext">Action called with additional data and MonoBehaviour instance.</param>
        public static void SubscribeUntilDestroy<TBehaviour, T, TTuple>(
            this Observable<T> observable,
            TBehaviour behaviour,
            TTuple tuple,
            Action<TTuple, TBehaviour> onNext)
            where TBehaviour : MonoBehaviour
        {
            observable.Subscribe((behaviour, tuple, onNext),
                    static (_, state) => state.onNext(state.tuple, state.behaviour))
                .RegisterTo(behaviour.destroyCancellationToken);
        }

        /// <summary>
        /// Subscribes to observable and automatically disposes on MonoBehaviour destruction.
        /// </summary>
        /// <typeparam name="TBehaviour">MonoBehaviour type.</typeparam>
        /// <typeparam name="T">Observable value type.</typeparam>
        /// <typeparam name="TTuple">Additional data type.</typeparam>
        /// <param name="observable">Observable to subscribe to.</param>
        /// <param name="behaviour">MonoBehaviour instance for disposal registration.</param>
        /// <param name="tuple">Additional data passed to the action.</param>
        /// <param name="onNext">Action called with observable value, MonoBehaviour instance, and additional data.</param>
        public static void SubscribeUntilDestroy<TBehaviour, T, TTuple>(
            this Observable<T> observable,
            TBehaviour behaviour,
            TTuple tuple,
            Action<T, TBehaviour, TTuple> onNext)
            where TBehaviour : MonoBehaviour
        {
            observable.Subscribe((behaviour, tuple, onNext),
                    static (value, state) => state.onNext(value, state.behaviour, state.tuple))
                .RegisterTo(behaviour.destroyCancellationToken);
        }

        /// <summary>
        /// Subscribes to the boolean observable and updates the selectable's interactable state until the selectable is destroyed.
        /// </summary>
        /// <param name="source">The boolean observable source.</param>
        /// <param name="selectable">The selectable UI element to update.</param>
        public static void SubscribeToInteractableUntilDestroy(this Observable<bool> source, Selectable selectable)
        {
            source.SubscribeToInteractable(selectable).RegisterTo(selectable.destroyCancellationToken);
        }

        /// <summary>
        /// Subscribes to the observable and updates the TextMeshProUGUI text until the text component is destroyed.
        /// </summary>
        /// <typeparam name="T">The type of value being observed.</typeparam>
        /// <param name="source">The observable source.</param>
        /// <param name="text">The TextMeshProUGUI component to update.</param>
        public static void SubscribeToTextUntilDestroy<T>(this Observable<T> source, TextMeshProUGUI text)
        {
            // ReSharper disable once HeapView.PossibleBoxingAllocation | We don't care about boxing here
            source.Subscribe(text, static (value, text) => text.text = value.ToString())
                .RegisterTo(text.destroyCancellationToken);
        }
    }
}