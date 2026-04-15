using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace CustomUtils.Runtime.Storage.Base
{
    public abstract class BaseCloudStorageProvider : BaseStorageProvider, ICloudStorageProvider
    {
        private readonly Dictionary<string, CancellationTokenSource> _pendingTokens = new();
        private readonly TimeSpan _debounceDelay;

        protected BaseCloudStorageProvider(TimeSpan debounceDelay)
        {
            _debounceDelay = debounceDelay;
        }

        public override async UniTask<bool> TrySaveAsync<TData>(string key, TData data, bool isForce = false)
        {
            if (_pendingTokens.TryGetValue(key, out var source))
            {
                source.Cancel();
                source.Dispose();
            }

            var tokenSource = new CancellationTokenSource();
            _pendingTokens[key] = tokenSource;

            if (!isForce)
                await UniTask.Delay(_debounceDelay, cancellationToken: tokenSource.Token);

            _pendingTokens.Remove(key);
            tokenSource.Dispose();
            return await OnTrySaveAsync(key, data);
        }

        protected abstract UniTask<bool> OnTrySaveAsync<TData>(string key, TData data);
    }
}