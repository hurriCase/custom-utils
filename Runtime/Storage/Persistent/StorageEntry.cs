using System;
using System.Threading;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CustomUtils.Runtime.Storage.Persistent
{
    internal sealed class StorageEntry<TData> : IStorageEntry, IDisposable
    {
        private readonly string _key;
        private readonly IStorageProvider _provider;

        private bool _savingEnabled;

        private TData _latestData;
        private bool _isSaveScheduled;

        private bool _hasUnflushedChanges;

#if UNITY_EDITOR
        private readonly bool _isKeyRegistered;
#endif

        internal StorageEntry(string key, IStorageProvider provider)
        {
            _key = key;
            _provider = provider ?? StorageProvider.Provider;

            if (_provider == null)
            {
                Debug.LogError($"[{nameof(StorageEntry<TData>)}::{nameof(StorageEntry<TData>)}] " +
                               $"No storage provider for key '{key}', the value won't be loaded or saved. " +
                               $"Call {nameof(StorageProvider)}.{nameof(StorageProvider.SetProvider)} at startup");
                return;
            }

#if UNITY_EDITOR
            _isKeyRegistered = StorageKeyRegistry.TryRegister(_provider, _key);
#endif
        }

        internal async UniTask<LoadResult<TData>> LoadAsync(CancellationToken token)
        {
            if (_provider == null)
                return LoadResult<TData>.Failed;

            var result = await _provider.TryLoadAsync<TData>(_key, token);

            if (result.Status == LoadStatus.Failed)
                Debug.LogError($"[{nameof(StorageEntry<TData>)}::{nameof(LoadAsync)}] " +
                               $"Failed to load key '{_key}', saving is disabled to keep stored data");

            return result;
        }

        internal void EnableSaving()
        {
            _savingEnabled = true;
        }

        internal UniTask<bool> SaveAsync(TData data, bool isForce = false)
        {
            if (_savingEnabled)
                return _provider.TrySaveAsync(_key, data, isForce);

            Debug.LogWarning($"[{nameof(StorageEntry<TData>)}::{nameof(SaveAsync)}] " +
                             $"Key '{_key}' wasn't saved, saving is disabled because loading it failed");
            return UniTask.FromResult(false);
        }

        internal void SaveIfEnabled(TData data)
        {
            if (!_savingEnabled)
                return;

            // The player loop doesn't run in edit mode, so editor values are saved right away
            if (!Application.isPlaying)
            {
                SaveAsync(data).Forget();
                return;
            }

            _latestData = data;

            if (!_hasUnflushedChanges)
            {
                _hasUnflushedChanges = true;
                StorageChangeTracker.MarkChanged(this);
            }

            if (_isSaveScheduled)
                return;

            _isSaveScheduled = true;
            SaveAtEndOfFrameAsync().Forget();
        }

        private async UniTaskVoid SaveAtEndOfFrameAsync()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            if (!_isSaveScheduled)
                return;

            _isSaveScheduled = false;
            SaveAsync(_latestData).Forget();
        }

        public void Flush()
        {
            if (!_hasUnflushedChanges)
                return;

            _hasUnflushedChanges = false;
            _isSaveScheduled = false;
            StorageChangeTracker.MarkFlushed(this);

            SaveAsync(_latestData, isForce: true).Forget();
        }

        public void Dispose()
        {
            Flush();

#if UNITY_EDITOR
            if (_isKeyRegistered)
                StorageKeyRegistry.Unregister(_provider, _key);
#endif
        }
    }
}
