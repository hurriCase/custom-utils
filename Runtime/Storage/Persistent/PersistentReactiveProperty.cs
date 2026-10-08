using System;
using System.Threading;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;

namespace CustomUtils.Runtime.Storage.Persistent
{
    /// <summary>
    /// A reactive property that automatically persists its value to storage.
    /// Call <see cref="InitializeAsync"/> to load the saved value.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property value</typeparam>
    [PublicAPI]
    public sealed class PersistentReactiveProperty<TProperty> : IDisposable
    {
        /// <summary>
        /// Gets the underlying reactive property.
        /// </summary>
        public ReactiveProperty<TProperty> Property { get; }

        private StorageEntry<TProperty> _entry;
        private readonly IDisposable _subscription;

        /// <summary>
        /// Gets or sets the current value. Setting it automatically saves the value to storage.
        /// </summary>
        public TProperty Value
        {
            get => Property.Value;
            set => Property.Value = value;
        }

        /// <summary>
        /// Subscribes to value changes, passing <paramref name="target"/> to avoid a closure allocation.
        /// </summary>
        /// <typeparam name="TTarget">The type of the state passed to <paramref name="onNext"/></typeparam>
        /// <param name="target">State passed to <paramref name="onNext"/> on every change</param>
        /// <param name="onNext">Callback invoked with the current value and <paramref name="target"/></param>
        /// <returns>A subscription that stops the callbacks when disposed</returns>
        public IDisposable Subscribe<TTarget>(TTarget target, Action<TProperty, TTarget> onNext)
            => Property.Subscribe((target, onNext),
                static (property, tuple) => tuple.onNext(property, tuple.target));

        /// <summary>
        /// Subscribes to value changes.
        /// </summary>
        /// <param name="onNext">Callback invoked with the current value on every change</param>
        /// <returns>A subscription that stops the callbacks when disposed</returns>
        public IDisposable Subscribe(Action<TProperty> onNext)
            => Property.Subscribe(onNext, static (property, action) => action(property));

        /// <summary>
        /// Exposes the property as an observable of its values.
        /// </summary>
        /// <returns>An observable that emits the current value and every change</returns>
        public Observable<TProperty> AsObservable() => Property.AsObservable();

        /// <summary>
        /// Creates the property with a default value. Call <see cref="InitializeAsync"/> to load the saved value.
        /// </summary>
        public PersistentReactiveProperty()
        {
            Property = new ReactiveProperty<TProperty>();
            _subscription = Property.Subscribe(this, static (value, self) => self._entry?.SaveIfEnabled(value));
        }

        /// <summary>
        /// Loads the saved value from storage and starts saving changes.
        /// Saving stays disabled if loading fails, so stored data isn't overwritten.
        /// </summary>
        /// <param name="key">Unique storage key for this property</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="defaultValue">Value to use when no saved data exists for the given key</param>
        /// <param name="provider">Storage provider to use instead of <see cref="StorageProvider.Provider"/>,
        /// e.g. <see cref="StorageProvider.Local"/> to keep the value on device only</param>
        public async UniTask InitializeAsync(
            string key,
            CancellationToken token = default,
            TProperty defaultValue = default,
            IStorageProvider provider = null)
        {
            _entry = new StorageEntry<TProperty>(key, provider);

            var result = await _entry.LoadAsync(token);
            Value = result.Status == LoadStatus.Loaded ? result.Data : defaultValue;

            if (result.Status == LoadStatus.Failed)
                return;

            _entry.EnableSaving();
        }

        /// <summary>
        /// Manually saves the current value to storage.
        /// The value is saved automatically when changed, so this is typically not needed.
        /// </summary>
        /// <param name="isForce">Save immediately, bypassing the cloud provider's debounce</param>
        public async UniTask SaveAsync(bool isForce = false)
        {
            await _entry.SaveAsync(Property.Value, isForce);
        }

        /// <summary>
        /// Disposes the property and stops automatic saving.
        /// This should be called when the property is no longer needed to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            _subscription.Dispose();
            _entry?.Dispose();
            Property.Dispose();
        }
    }
}
