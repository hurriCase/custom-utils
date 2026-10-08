using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using ObservableCollections;
using R3;

namespace CustomUtils.Runtime.Storage.Persistent
{
    /// <summary>
    /// An observable list that automatically persists its contents to storage.
    /// Call <see cref="InitializeAsync"/> to load the saved values.
    /// </summary>
    /// <typeparam name="TValue">The type of elements in the list</typeparam>
    [PublicAPI]
    public sealed class PersistentObservableList<TValue> : ObservableList<TValue>, IDisposable
    {
        private StorageEntry<List<TValue>> _entry;
        private readonly IDisposable _subscription;
        private readonly List<TValue> _serializationBuffer = new();

        /// <summary>
        /// Creates an empty list. Call <see cref="InitializeAsync"/> to load the saved values.
        /// </summary>
        public PersistentObservableList()
        {
            _subscription = this.ObserveChanged()
                .Subscribe(this, static (changedEvent, self) => self.HandleCollectionChanged(changedEvent));
        }

        /// <summary>
        /// Loads any saved values from storage and starts saving changes.
        /// Saving stays disabled if loading fails, so stored data isn't overwritten.
        /// </summary>
        /// <param name="key">Unique storage key for this list</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="defaultValues">Initial values to populate the list when no saved data exists for the given key</param>
        /// <param name="provider">Storage provider to use instead of <see cref="StorageProvider.Provider"/>,
        /// e.g. <see cref="StorageProvider.Local"/> to keep the value on device only</param>
        public async UniTask InitializeAsync(
            string key,
            CancellationToken token,
            IReadOnlyList<TValue> defaultValues = null,
            IStorageProvider provider = null)
        {
            _entry = new StorageEntry<List<TValue>>(key, provider);

            var result = await _entry.LoadAsync(token);
            if (result.Status == LoadStatus.Failed)
                return;

            IEnumerable<TValue> values = result.Status == LoadStatus.Loaded ? result.Data : defaultValues;
            if (values != null)
                AddRange(values);

            _entry.EnableSaving();
        }

        /// <summary>
        /// Manually saves the current list contents to storage.
        /// The list is saved automatically when changed, so this is typically not needed.
        /// </summary>
        /// <param name="isForce">Save immediately, bypassing the cloud provider's debounce</param>
        public async UniTask SaveAsync(bool isForce = false)
        {
            await _entry.SaveAsync(_serializationBuffer, isForce);
        }

        private void HandleCollectionChanged(CollectionChangedEvent<TValue> changedEvent)
        {
            ApplyChangeToBuffer(changedEvent);
            _entry?.SaveIfEnabled(_serializationBuffer);
        }

        private void ApplyChangeToBuffer(CollectionChangedEvent<TValue> changedEvent)
        {
            switch (changedEvent.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _serializationBuffer.Insert(changedEvent.NewStartingIndex, changedEvent.NewItem);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    _serializationBuffer.RemoveAt(changedEvent.OldStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    _serializationBuffer[changedEvent.NewStartingIndex] = changedEvent.NewItem;
                    break;

                case NotifyCollectionChangedAction.Move:
                    _serializationBuffer.RemoveAt(changedEvent.OldStartingIndex);
                    _serializationBuffer.Insert(changedEvent.NewStartingIndex, changedEvent.NewItem);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    _serializationBuffer.Clear();
                    _serializationBuffer.AddRange(this);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(changedEvent.Action), changedEvent.Action,
                        $"[{nameof(PersistentObservableList<TValue>)}::{nameof(ApplyChangeToBuffer)}]" +
                        " Unexpected action for a list change.");
            }
        }

        /// <summary>
        /// Disposes the list and stops automatic saving.
        /// This should be called when the list is no longer needed to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            _subscription.Dispose();
            _entry?.Dispose();
        }
    }
}
