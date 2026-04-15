using System.Threading;
using Cysharp.Threading.Tasks;

namespace CustomUtils.Runtime.Storage.Base
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for storage providers. Handles serialization, caching, and error logging.
    /// Extend this to implement a custom storage backend.
    /// </summary>
    public abstract class BaseStorageProvider : IStorageProvider
    {
        public abstract UniTask<bool> TrySaveAsync<TData>(string key, TData data, bool isForce = false);
        public abstract UniTask<TData> LoadAsync<TData>(string key, CancellationToken token);
        public abstract UniTask<bool> HasKeyAsync(string key, CancellationToken token);
        public abstract UniTask<bool> TryDeleteKeyAsync(string key, CancellationToken token);
        public abstract UniTask<bool> TryDeleteAllAsync(CancellationToken token);
    }
}