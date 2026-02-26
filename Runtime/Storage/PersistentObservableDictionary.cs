#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using ObservableCollections;
using R3;
using UnityEngine;

namespace CustomUtils.Runtime.Storage
{
    /// <summary>
    /// An observable dictionary that automatically persists its contents to storage.
    /// Must call <see cref="InitializeAsync"/> before use to load saved values.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary</typeparam>
    [PublicAPI]
    public sealed class PersistentObservableDictionary<TKey, TValue> :
        IObservableCollection<KeyValuePair<TKey, TValue>>,
        IReadOnlyDictionary<TKey, TValue>,
        IDisposable
        where TKey : notnull
    {
        /// <summary>
        /// Gets the underlying observable dictionary.
        /// </summary>
        public ObservableDictionary<TKey, TValue>? Dictionary { get; private set; }

        /// <summary>
        /// Gets the number of key-value pairs in the dictionary.
        /// </summary>
        public int Count => Dictionary!.Count;

        /// <summary>
        /// Gets the object used to synchronize access to the dictionary.
        /// </summary>
        public object SyncRoot => Dictionary!.SyncRoot;

        /// <summary>
        /// Occurs when the dictionary changes. Subscribe to react to add, remove, replace, and reset operations.
        /// </summary>
        public event NotifyCollectionChangedEventHandler<KeyValuePair<TKey, TValue>>? CollectionChanged
        {
            add => Dictionary!.CollectionChanged += value;
            remove => Dictionary!.CollectionChanged -= value;
        }

        private string _key = string.Empty;
        private IStorageProvider? _provider;
        private IDisposable? _subscription;
        private bool _savingEnabled;
        private readonly Dictionary<TKey, TValue> _serializationBuffer = new();

        public bool ContainsKey(TKey key) => Dictionary!.ContainsKey(key);
        public bool TryGetValue(TKey key, out TValue value) => Dictionary!.TryGetValue(key, out value);

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// Setting this value will automatically save the dictionary to storage.
        /// </summary>
        public TValue this[TKey key]
        {
            get => Dictionary![key];
            set => Dictionary![key] = value;
        }

        public IEnumerable<TKey> Keys => ((IReadOnlyDictionary<TKey, TValue>)Dictionary!).Keys;
        public IEnumerable<TValue> Values => ((IReadOnlyDictionary<TKey, TValue>)Dictionary!).Values;

        /// <summary>
        /// Initializes the dictionary by loading any saved values from storage.
        /// This method must be called before using the dictionary.
        /// </summary>
        /// <param name="key">Unique storage key for this dictionary</param>
        /// <param name="token">Cancellation token</param>
        public async UniTask InitializeAsync(string key, CancellationToken token)
        {
            _key = key;
            _provider = ServiceProvider.Provider;
            Dictionary = new ObservableDictionary<TKey, TValue>();

            _subscription = Dictionary.ObserveChanged()
                .Where(this, static (_, self) => self._savingEnabled)
                .Subscribe(this, static (_, self) => self.SaveAsync().Forget());

            try
            {
                var loaded = await _provider.LoadAsync<Dictionary<TKey, TValue>>(_key, token);
                if (loaded != null)
                    foreach (var (loadedKey, loadedValue) in loaded)
                        Dictionary[loadedKey] = loadedValue;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogError($"[PersistentObservableDictionary::InitializeAsync] " +
                               $"Failed to load key '{_key}': {e.Message}");
            }
            finally
            {
                _savingEnabled = true;
            }
        }

        /// <summary>
        /// Manually saves the current dictionary contents to storage.
        /// Note: Dictionary is automatically saved when changed, so this is typically not needed.
        /// </summary>
        /// <returns>A task that completes when the save operation is finished</returns>
        public async UniTask SaveAsync()
        {
            _serializationBuffer.Clear();
            foreach (var (key, value) in Dictionary!)
                _serializationBuffer[key] = value;

            await _provider!.TrySaveAsync(_key, _serializationBuffer);
        }

        /// <summary>
        /// Creates a synchronized view of the dictionary with a transform applied to each element.
        /// </summary>
        /// <typeparam name="TView">The type of the view elements</typeparam>
        /// <param name="transform">Function to transform each key-value pair into a view element</param>
        /// <returns>A synchronized view of the dictionary</returns>
        public ISynchronizedView<KeyValuePair<TKey, TValue>, TView> CreateView<TView>(
            Func<KeyValuePair<TKey, TValue>, TView> transform)
            => Dictionary!.CreateView(transform);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary!.GetEnumerator();

        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Disposes the dictionary and stops automatic saving.
        /// This should be called when the dictionary is no longer needed to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}