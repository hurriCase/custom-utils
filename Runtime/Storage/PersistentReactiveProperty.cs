using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using UnityEngine;

namespace CustomUtils.Runtime.Storage
{
    /// <summary>
    /// A reactive property that automatically persists its value to storage.
    /// Must call <see cref="InitAsync"/> before use to load saved values.
    /// </summary>
    /// <typeparam name="TProperty">The type of value to store and persist</typeparam>
    [PublicAPI]
    public sealed class PersistentReactiveProperty<TProperty> : IDisposable
    {
        /// <summary>
        /// Gets the underlying reactive property.
        /// </summary>

        public ReactiveProperty<TProperty> Property { get; private set; }

        private string _key;
        private IDisposable _subscription;
        private IStorageProvider _provider;
        private bool _savingEnabled;

        /// <summary>
        /// Gets or sets the current value of the property.
        /// Setting this value will automatically save it to storage.
        /// </summary>
        /// <value>The current value stored in the property</value>

        public TProperty Value
        {
            get => Property.Value;
            set => Property.Value = value;
        }

        /// <summary>
        /// Subscribes to value changes with a target object and callback.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target object to pass to the callback</typeparam>
        /// <param name="target">Target object to pass to the callback</param>
        /// <param name="onNext">Action to execute when value changes. Receives the new value and target object.</param>
        /// <returns>A disposable subscription that can be used to unsubscribe</returns>
        public IDisposable Subscribe<TTarget>(TTarget target, Action<TProperty, TTarget> onNext)
            => Property.Subscribe((target, onNext),
                static (property, tuple) => tuple.onNext(property, tuple.target));

        /// <summary>
        /// Subscribes to value changes with a simple callback.
        /// </summary>
        /// <param name="onNext">Action to execute when value changes. Receives the new value.</param>
        /// <returns>A disposable subscription that can be used to unsubscribe</returns>
        public IDisposable Subscribe(Action<TProperty> onNext)
            => Property.Subscribe(onNext, static (property, action) => action(property));

        /// <summary>
        /// Gets the observable stream for this property.
        /// Use this for advanced reactive operations like filtering, mapping, or combining with other observables.
        /// </summary>
        /// <returns>An observable stream of value changes</returns>
        public Observable<TProperty> AsObservable() => Property.AsObservable();

        /// <summary>
        /// Initializes the property by loading any saved value from storage.
        /// This method must be called before using the property.
        /// </summary>
        /// <param name="key">Unique storage key for this property</param>
        /// <param name="token">Cancellation token.</param>
        /// <param name="defaultValue">Default value to use if no saved value exists or loading fails</param>
        /// <returns>A task that completes when initialization is finished</returns>
        /// <exception cref="InvalidOperationException">Thrown if called multiple times</exception>
        public async UniTask InitAsync(string key, CancellationToken token = default, TProperty defaultValue = default)
        {
            _provider = ServiceProvider.Provider;

            _key = key;
            Property = new ReactiveProperty<TProperty>(defaultValue);

            _subscription = Property
                .Where(this, static (_, self) => self._savingEnabled)
                .Subscribe(this, static (_, self) => self.SaveAsync().Forget());

            try
            {
                var loaded = await _provider.LoadAsync<TProperty>(_key, token);
                if (loaded != null && EqualityComparer<TProperty>.Default.Equals(loaded, default) is false)
                    Property.Value = loaded;
            }
            catch (Exception ex)
            {
                Debug.LogError("[PersistentReactiveProperty::InitializeAsync] " +
                               $"Failed to load key '{_key}': {ex.Message}");
            }
            finally
            {
                _savingEnabled = true;
            }
        }

        /// <summary>
        /// Manually saves the current value to storage.
        /// Note: Values are automatically saved when changed, so this is typically not needed.
        /// </summary>
        /// <returns>A task that completes when the save operation is finished</returns>
        public async UniTask SaveAsync() => await _provider.TrySaveAsync(_key, Property.Value);

        /// <summary>
        /// Disposes the property and stops automatic saving.
        /// This should be called when the property is no longer needed to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            _subscription?.Dispose();
            Property?.Dispose();
        }
    }
}