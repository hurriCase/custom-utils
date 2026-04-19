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

        protected CloudStorageProviderBase(TimeSpan debounceDelay)
        {
            _debounceDelay = debounceDelay;
        }

        public override async UniTask<bool> TrySaveAsync<TData>(string key, TData data, bool isForce = false)
        {
            var firstSave = !_pendingTokens.TryGetValue(key, out var tokenSource);

            var token = CancellationExtensions.GetFreshToken(ref tokenSource);
            _pendingTokens[key] = tokenSource;

            if (isForce || firstSave)
            {
                DeleteKeyAfterDelayAsync(tokenSource, key, token).Forget();
                return await base.TrySaveAsync(key, data);
            }

            await UniTask.Delay(_debounceDelay, cancellationToken: token).SuppressCancellationThrow();

            if (!token.IsCancellationRequested)
                return await base.TrySaveAsync(key, data);

            CleanSave(tokenSource, key);
            return false;
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
            tokenSource.Dispose();
            _pendingTokens.Remove(key);
        }
    }
}