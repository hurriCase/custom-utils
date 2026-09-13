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
    /// An observable list that automatically persists its contents to storage.
    /// Must call <see cref="InitializeAsync"/> before use to load saved values.
    /// </summary>
    /// <typeparam name="TValue">The type of elements in the list</typeparam>
    [PublicAPI]
    public sealed class PersistentObservableList<TValue> :
        IObservableCollection<TValue>,
        IReadOnlyList<TValue>,
        IDisposable
    {
        /// <summary>
        /// Gets the underlying observable list.
        /// </summary>
        public ObservableList<TValue>? List { get; private set; }

        /// <summary>
        /// Gets the number of elements in the list.
        /// </summary>
        public int Count => List!.Count;

        /// <summary>
        /// Gets the object used to synchronize access to the list.
        /// </summary>
        public object SyncRoot => List!.SyncRoot;

        /// <summary>
        /// Occurs when the list changes. Subscribe to react to add, remove, replace, and reset operations.
        /// </summary>
        public event NotifyCollectionChangedEventHandler<TValue>? CollectionChanged
        {
            add => List!.CollectionChanged += value;
            remove => List!.CollectionChanged -= value;
        }

        private string _key = string.Empty;
        private IStorageProvider? _provider;
        private IDisposable? _subscription;
        private bool _savingEnabled;
        private readonly List<TValue> _serializationBuffer = new();

        public bool Contains(TValue value) => List!.Contains(value);
        public int IndexOf(TValue value) => List!.IndexOf(value);

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// Setting this value will automatically save the list to storage.
        /// </summary>
        public TValue this[int index]
        {
            get => List![index];
            set => List![index] = value;
        }

        /// <summary>
        /// Adds an element to the end of the list. Automatically saves the list to storage.
        /// </summary>
        public void Add(TValue value) => List!.Add(value);

        /// <summary>
        /// Inserts an element at the specified index. Automatically saves the list to storage.
        /// </summary>
        public void Insert(int index, TValue value) => List!.Insert(index, value);

        /// <summary>
        /// Removes the first occurrence of the specified element. Automatically saves the list to storage.
        /// </summary>
        public bool Remove(TValue value) => List!.Remove(value);

        /// <summary>
        /// Removes the element at the specified index. Automatically saves the list to storage.
        /// </summary>
        public void RemoveAt(int index) => List!.RemoveAt(index);

        /// <summary>
        /// Removes all elements from the list. Automatically saves the list to storage.
        /// </summary>
        public void Clear() => List!.Clear();

        /// <summary>
        /// Initializes the list by loading any saved values from storage.
        /// This method must be called before using the list.
        /// </summary>
        /// <param name="key">Unique storage key for this list</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="defaultValues">Initial values to populate the list when no saved data exists for the given key</param>
        public async UniTask InitializeAsync(string key, CancellationToken token, IReadOnlyList<TValue>? defaultValues = null)
        {
            _key = key;
            _provider = StorageProvider.Provider;
            List = new ObservableList<TValue>();

            _subscription = List.ObserveChanged()
                .Where(this, static (_, self) => self._savingEnabled)
                .Subscribe(this, static (_, self) => self.SaveAsync().Forget());

            try
            {
                var hasKey = await _provider.HasKeyAsync(_key, token);
                if (hasKey)
                {
                    var loaded = await _provider.LoadAsync<List<TValue>>(_key, token);
                    if (loaded != null)
                        foreach (var value in loaded)
                            List.Add(value);
                }
                else if (defaultValues != null)
                    foreach (var value in defaultValues)
                        List.Add(value);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError("[PersistentObservableList::InitializeAsync] " +
                               $"Failed to load key '{_key}': {exception.Message}");
            }
            finally
            {
                _savingEnabled = true;
            }
        }

        /// <summary>
        /// Manually saves the current list contents to storage.
        /// Note: List is automatically saved when changed, so this is typically not needed.
        /// </summary>
        /// <returns>A task that completes when the save operation is finished</returns>
        public async UniTask SaveAsync(bool isForce = false)
        {
            _serializationBuffer.Clear();
            _serializationBuffer.AddRange(List!);

            await _provider!.TrySaveAsync(_key, _serializationBuffer, isForce);
        }

        /// <summary>
        /// Creates a synchronized view of the list with a transform applied to each element.
        /// </summary>
        /// <typeparam name="TView">The type of the view elements</typeparam>
        /// <param name="transform">Function to transform each element into a view element</param>
        /// <returns>A synchronized view of the list</returns>
        public ISynchronizedView<TValue, TView> CreateView<TView>(Func<TValue, TView> transform)
            => List!.CreateView(transform);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerator<TValue> GetEnumerator() => List!.GetEnumerator();

        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Disposes the list and stops automatic saving.
        /// This should be called when the list is no longer needed to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}