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
    /// After changing data inside the stored value, call <see cref="ReactiveProperty{T}.ForceNotify"/>
    /// to notify subscribers and save it.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property value</typeparam>
    [PublicAPI]
    public sealed class PersistentReactiveProperty<TProperty> : ReactiveProperty<TProperty>
    {
        private StorageEntry<TProperty> _entry;

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
            await _entry.SaveAsync(Value, isForce);
        }

        protected override void OnNextCore(TProperty value)
        {
            _entry?.SaveIfEnabled(value);
            base.OnNextCore(value);
        }

        protected override void DisposeCore()
        {
            _entry?.Dispose();
            base.DisposeCore();
        }
    }
}
