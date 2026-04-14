#if CRAZY_GAMES
using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using CrazyGames;
using CustomUtils.Runtime.Serializer;

namespace CustomUtils.Runtime.Storage.Providers
{
    [PublicAPI]
    public sealed class CrazyGamesStorageProvider : BaseCloudStorageProvider
{
    private readonly IStringSerializer _serializer;
    private readonly Dictionary<string, string> _cache = new();

    public CrazyGamesStorageProvider(IStringSerializer serializer, TimeSpan debounceDelay) : base(debounceDelay)
    {
        _serializer = serializer;
    }

    protected override UniTask<bool> OnTrySaveAsync<TData>(string key, TData data)
    {
        try
        {
            var serialized = _serializer.SerializeToString(data);
            _cache[key] = serialized;
            CrazySDK.Data.SetString(key, serialized);
            Logger.Log($"[{nameof(CrazyGamesStorageProvider)}::OnTrySaveAsync] Saved data for key '{key}'");
            return UniTask.FromResult(true);
        }
        catch (Exception e)
        {
            Logger.LogException(e);
            Logger.LogError($"[{nameof(CrazyGamesStorageProvider)}::OnTrySaveAsync] Error during saving data: {e.Message}");
            return UniTask.FromResult(false);
        }
    }

    public override UniTask<TData> LoadAsync<TData>(string key, CancellationToken token)
    {
        try
        {
            if (_cache.TryGetValue(key, out var cachedData))
                return UniTask.FromResult(_serializer.DeserializeFromString<TData>(cachedData));

            var raw = CrazySDK.Data.GetString(key, null);
            if (raw == null)
                return UniTask.FromResult(default(TData));

            var data = _serializer.DeserializeFromString<TData>(raw);
            _cache[key] = raw;
            return UniTask.FromResult(data);
        }
        catch (Exception e)
        {
            Logger.LogException(e);
            Logger.LogError($"[{nameof(CrazyGamesStorageProvider)}::LoadAsync] Error loading data: {e.Message}");
            return UniTask.FromResult(default(TData));
        }
    }

    public override UniTask<bool> HasKeyAsync(string key, CancellationToken token)
        => UniTask.FromResult(_cache.ContainsKey(key) || CrazySDK.Data.HasKey(key));

    public override UniTask<bool> TryDeleteKeyAsync(string key, CancellationToken token)
    {
        try
        {
            _cache.Remove(key);
            CrazySDK.Data.DeleteKey(key);
            Logger.Log($"[{nameof(CrazyGamesStorageProvider)}::TryDeleteKeyAsync] Deleted key '{key}'");
            return UniTask.FromResult(true);
        }
        catch (Exception e)
        {
            Logger.LogException(e);
            Logger.LogError($"[{nameof(CrazyGamesStorageProvider)}::TryDeleteKeyAsync] Error deleting key: {e.Message}");
            return UniTask.FromResult(false);
        }
    }

    public override UniTask<bool> TryDeleteAllAsync(CancellationToken token)
    {
        _cache.Clear();
        CrazySDK.Data.DeleteAll();
        return UniTask.FromResult(true);
    }
}
}
#endif