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
    /// An observable dictionary that automatically persists its contents to storage.
    /// Created via <see cref="CreateAsync"/>, which loads saved values before returning.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary</typeparam>
    [PublicAPI]
    public sealed class PersistentObservableDictionary<TKey, TValue> : ObservableDictionary<TKey, TValue>, IDisposable
    {
        private readonly StorageEntry<Dictionary<TKey, TValue>> _entry;
        private readonly IDisposable _subscription;
        private readonly Dictionary<TKey, TValue> _serializationBuffer = new();

        private PersistentObservableDictionary(string key, IStorageProvider provider)
        {
            _entry = new StorageEntry<Dictionary<TKey, TValue>>(key, provider);

            _subscription = this.ObserveChanged()
                .Subscribe(this, static (changedEvent, self) => self.HandleCollectionChanged(changedEvent));
        }

        /// <summary>
        /// Creates the dictionary and loads any saved values from storage.
        /// </summary>
        /// <param name="key">Unique storage key for this dictionary</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="defaultValues">Initial values to populate the dictionary when no saved data exists for the given key</param>
        /// <param name="provider">Storage provider to use instead of <see cref="StorageProvider.Provider"/>,
        /// e.g. <see cref="StorageProvider.Local"/> to keep the value on device only</param>
        /// <returns>The dictionary populated with saved values, or with <paramref name="defaultValues"/> if none exist</returns>
        public static async UniTask<PersistentObservableDictionary<TKey, TValue>> CreateAsync(
            string key,
            CancellationToken token,
            Dictionary<TKey, TValue> defaultValues = null,
            IStorageProvider provider = null)
        {
            var dictionary = new PersistentObservableDictionary<TKey, TValue>(key, provider);

            var result = await dictionary._entry.LoadAsync(token);
            if (result.Status == LoadStatus.Failed)
                return dictionary;

            var entries = result.Status == LoadStatus.Loaded ? result.Data : defaultValues;
            if (entries != null)
                foreach (var (entryKey, entryValue) in entries)
                    dictionary.Add(entryKey, entryValue);

            dictionary._entry.EnableSaving();
            return dictionary;
        }

        /// <summary>
        /// Manually saves the current dictionary contents to storage.
        /// The dictionary is saved automatically when changed, so this is typically not needed.
        /// </summary>
        /// <param name="isForce">Save immediately, bypassing the cloud provider's debounce</param>
        public async UniTask SaveAsync(bool isForce = false)
        {
            await _entry.SaveAsync(_serializationBuffer, isForce);
        }

        private void HandleCollectionChanged(CollectionChangedEvent<KeyValuePair<TKey, TValue>> changedEvent)
        {
            ApplyChangeToBuffer(changedEvent);
            _entry.SaveIfEnabled(_serializationBuffer);
        }

        private void ApplyChangeToBuffer(CollectionChangedEvent<KeyValuePair<TKey, TValue>> changedEvent)
        {
            switch (changedEvent.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    _serializationBuffer[changedEvent.NewItem.Key] = changedEvent.NewItem.Value;
                    break;

                case NotifyCollectionChangedAction.Remove:
                    _serializationBuffer.Remove(changedEvent.OldItem.Key);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    _serializationBuffer.Clear();
                    foreach (var item in this)
                        _serializationBuffer[item.Key] = item.Value;
                    break;

                case NotifyCollectionChangedAction.Move:
                default:
                    throw new ArgumentOutOfRangeException(nameof(changedEvent.Action), changedEvent.Action,
                        $"[{nameof(PersistentObservableDictionary<TKey, TValue>)}::{nameof(ApplyChangeToBuffer)}]" +
                        " Unexpected action for a dictionary change.");
            }
        }

        /// <summary>
        /// Disposes the dictionary and stops automatic saving.
        /// This should be called when the dictionary is no longer needed to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            _subscription.Dispose();
            _entry.Dispose();
        }
    }
}
