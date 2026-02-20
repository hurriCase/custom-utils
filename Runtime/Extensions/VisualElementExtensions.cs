using System;
using System.Diagnostics.CodeAnalysis;
using CustomUtils.Unsafe;
using JetBrains.Annotations;
using R3;
using UnityEngine.UIElements;

#if IS_DEBUG
using UnityEngine;
#endif

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="VisualElement"/>.
    /// </summary>
    [PublicAPI]
    public static class VisualElementExtensions
    {
        /// <summary>
        /// Shows or hides the visual element by setting its display style.
        /// </summary>
        /// <param name="visualElement">The visual element to show or hide.</param>
        /// <param name="isActive">True to show the element; false to hide it.</param>
        public static void SetActive(this VisualElement visualElement, bool isActive)
        {
            visualElement.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Creates an observable stream from a button's click events.
        /// </summary>
        /// <param name="visualElement">The visual element containing the button.</param>
        /// <param name="buttonName">The name of the button to observe.</param>
        /// <returns>An observable that emits when the button is clicked.</returns>

        // There is no overload that accepts extra parameters for FromEvent
        [SuppressMessage("ReSharper", "HeapView.DelegateAllocation")]
        [SuppressMessage("ReSharper", "HeapView.ClosureAllocation")]
        public static Observable<Unit> CreateButtonObservable(this VisualElement visualElement, string buttonName)
        {
            var playButton = visualElement.Q<Button>(buttonName);

            return Observable.FromEvent(
                handler => playButton.clicked += handler,
                handler => playButton.clicked -= handler
            );
        }

        /// <summary>
        /// Binds the visibility of a <see cref="VisualElement"/> to a reactive property,
        /// based on a comparison with a target enum value.
        /// </summary>
        /// <param name="element">The <see cref="VisualElement"/> whose visibility will be controlled.</param>
        /// <param name="source">The reactive property that provides the value to compare against the target enum.</param>
        /// <param name="targetEnum">The target enum value that determines the visibility of the element.</param>
        /// <typeparam name="T">The enum type used for the comparison.</typeparam>
        public static void BindVisibilityTo<T>(
            this VisualElement element,
            ReadOnlyReactiveProperty<T> source,
            T targetEnum) where T : unmanaged, Enum
        {
            var targetInt = UnsafeEnumConverter<T>.ToInt32(targetEnum);

            var subscription = source.Subscribe((element, targetInt), static (newValue, tuple) =>
            {
                var newInt = UnsafeEnumConverter<T>.ToInt32(newValue);
                tuple.element.style.display = newInt == tuple.targetInt ? DisplayStyle.Flex : DisplayStyle.None;
            });

            element.RegisterDetachCallback(subscription, static subscription => subscription.Dispose());
        }

        /// <summary>
        /// Registers a callback to be invoked when the specified <see cref="VisualElement"/> is detached from the panel.
        /// </summary>
        /// <param name="element">The VisualElement to register the callback on.</param>
        /// <param name="source">The source data or object to be passed to the callback when invoked.</param>
        /// <param name="callback">The callback action to execute when the VisualElement is detached from the panel.</param>
        /// <typeparam name="TSource">The type of the source parameter passed to the callback.</typeparam>
        public static void RegisterDetachCallback<TSource>(
            this VisualElement element,
            TSource source,
            Action<TSource> callback)
        {
            element.RegisterCallback<DetachFromPanelEvent, (TSource source, Action<TSource> callback)>(
                static (_, tuple) => tuple.callback.Invoke(tuple.source), (source, callback));
        }

        /// <summary>
        /// Attempts to query a child element of the specified type.
        /// </summary>
        /// <typeparam name="TElement">The type of the element to query for.</typeparam>
        /// <param name="element">The visual element to query from.</param>
        /// <param name="queriedElement">The queried element if found; otherwise, null.</param>
        /// <param name="name">The name of the element to query for.</param>
        /// <param name="className">The USS class name of the element to query for.</param>
        /// <returns>True if the element was found; otherwise, false.</returns>
        public static bool TryQ<TElement>(
            this VisualElement element,
            out TElement queriedElement,
            string name = null,
            string className = null)
            where TElement : VisualElement
        {
            queriedElement = element.Q<TElement>(name, className);
            var wasFound = queriedElement != null;

#if IS_DEBUG
            if (wasFound is false)
                Debug.LogWarning($"[BasePopupFieldExtensions::TryQ] Element of {typeof(TElement)} type wasn't found");
#endif

            return wasFound;
        }
    }
}