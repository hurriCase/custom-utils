using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Extensions;
using Cysharp.Threading.Tasks;

namespace CustomUtils.Runtime.Storage.Base
{
    public abstract class CloudStorageProviderBase<TCached> : StorageProviderBase<TCached>, ICloudStorageProvider
    {
        private readonly Dictionary<string, CancellationTokenSource> _pendingTokens = new();
        private readonly TimeSpan _debounceDelay;

        private readonly Dictionary<string, SemaphoreSlim> _uploadLocks = new();
        private readonly Dictionary<string, int> _saveSequences = new();
        private readonly Dictionary<string, int> _uploadedSequences = new();

        protected CloudStorageProviderBase(TimeSpan debounceDelay)
        {
            _debounceDelay = debounceDelay;
        }

        public override async UniTask<bool> TrySaveAsync<TData>(string key, TData data, bool isForce = false)
        {
            var sequence = _saveSequences[key] = _saveSequences.GetValueOrDefault(key) + 1;
            var firstSave = !_pendingTokens.TryGetValue(key, out var tokenSource);

            var token = CancellationExtensions.GetFreshToken(ref tokenSource);
            _pendingTokens[key] = tokenSource;

            if (isForce || firstSave)
            {
                DeleteKeyAfterDelayAsync(tokenSource, key, token).Forget();
                return await UploadAsync(key, data, sequence);
            }

            await UniTask.Delay(_debounceDelay, cancellationToken: token).SuppressCancellationThrow();

            if (!token.IsCancellationRequested)
                return await UploadAsync(key, data, sequence);

            CleanSave(tokenSource, key);
            return false;
        }

        // Uploads of one key run one at a time, and an upload is skipped once a newer save of that key
        // has been uploaded. Otherwise a slow older upload could finish last and leave an outdated value in the cloud.
        private async UniTask<bool> UploadAsync<TData>(string key, TData data, int sequence)
        {
            var uploadLock = GetUploadLock(key);
            await uploadLock.WaitAsync();

            try
            {
                if (_uploadedSequences.GetValueOrDefault(key) > sequence)
                    return false;

                if (!await base.TrySaveAsync(key, data))
                    return false;

                _uploadedSequences[key] = sequence;
                return true;
            }
            finally
            {
                uploadLock.Release();
            }
        }

        private SemaphoreSlim GetUploadLock(string key)
        {
            if (!_uploadLocks.TryGetValue(key, out var uploadLock))
                _uploadLocks[key] = uploadLock = new SemaphoreSlim(1, 1);

            return uploadLock;
        }

        private async UniTaskVoid DeleteKeyAfterDelayAsync(
            CancellationTokenSource tokenSource,
            string key,
            CancellationToken token)
        {
            await UniTask.Delay(_debounceDelay, cancellationToken: token).SuppressCancellationThrow();
            if (!token.IsCancellationRequested)
                CleanSave(tokenSource, key);
        }

        private void CleanSave(CancellationTokenSource tokenSource, string key)
        {
            if (_pendingTokens.TryGetValue(key, out var current) && current == tokenSource)
                _pendingTokens.Remove(key);

            tokenSource.Dispose();
        }
    }
}
