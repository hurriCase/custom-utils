using JetBrains.Annotations;
using ObservableCollections;
using R3;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IReadOnlyObservableList{T}"/>.
    /// </summary>
    [PublicAPI]
    public static class ReadOnlyObservableListExtensions
    {
        /// <summary>
        /// Observes all current and future items in the list.
        /// </summary>
        /// <remarks>
        /// Immediately emits all existing items upon subscription, then continues to emit
        /// any changed items.
        /// </remarks>
        /// <typeparam name="T">The type of the list items.</typeparam>
        /// <param name="list">The observable list to observe.</param>
        /// <returns>An observable sequence of items, starting with the current state.</returns>
        public static Observable<T> ObserveChangedWithCurrentState<T>(
            this IReadOnlyObservableList<T> list) =>
            Observable.Create<T, IReadOnlyObservableList<T>>(
                list,
                static (observer, list) =>
                {
                    foreach (var item in list)
                        observer.OnNext(item);

                    return list.ObserveChanged()
                        .Select(static changedEvent => changedEvent.NewItem)
                        .Subscribe(observer);
                });
    }
}