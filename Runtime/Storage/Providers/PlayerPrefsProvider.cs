using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Serializer;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Storage.Providers
{
    [PublicAPI]
    public sealed class PlayerPrefsProvider : BaseStorageProvider
{
    private readonly IStringSerializer _serializer;
    private readonly Dictionary<string, string> _cache = new();

    public PlayerPrefsProvider(IStringSerializer serializer)
    {
        _serializer = serializer;
    }

    public override UniTask<bool> TrySaveAsync<TData>(string key, TData data)
    {
        try
        {
            var serialized = _serializer.SerializeToString(data);
            _cache[key] = serialized;
            PlayerPrefs.SetString(key, serialized);
            PlayerPrefs.Save();
            Logger.Log($"[{nameof(PlayerPrefsProvider)}::TrySaveAsync] Saved data for key '{key}'");
            return UniTask.FromResult(true);
        }
        catch (Exception e)
        {
            Logger.LogException(e);
            Logger.LogError($"[{nameof(PlayerPrefsProvider)}::TrySaveAsync] Error during saving data: {e.Message}");
            return UniTask.FromResult(false);
        }
    }

    public override UniTask<TData> LoadAsync<TData>(string key, CancellationToken token)
    {
        try
        {
            if (_cache.TryGetValue(key, out var cachedData))
                return UniTask.FromResult(_serializer.DeserializeFromString<TData>(cachedData));

            var raw = PlayerPrefs.GetString(key, null);
            if (raw == null)
                return UniTask.FromResult(default(TData));

            var data = _serializer.DeserializeFromString<TData>(raw);
            _cache[key] = raw;
            Logger.Log($"[{nameof(PlayerPrefsProvider)}::LoadAsync] Loaded data for key '{key}'");
            return UniTask.FromResult(data);
        }
        catch (Exception e)
        {
            Logger.LogException(e);
            Logger.LogError($"[{nameof(PlayerPrefsProvider)}::LoadAsync] Error loading data: {e.Message}");
            return UniTask.FromResult(default(TData));
        }
    }

    public override UniTask<bool> HasKeyAsync(string key, CancellationToken token)
        => UniTask.FromResult(_cache.ContainsKey(key) || PlayerPrefs.HasKey(key));

    public override UniTask<bool> TryDeleteKeyAsync(string key, CancellationToken token)
    {
        try
        {
            _cache.Remove(key);
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            Logger.Log($"[{nameof(PlayerPrefsProvider)}::TryDeleteKeyAsync] Deleted key '{key}'");
            return UniTask.FromResult(true);
        }
        catch (Exception e)
        {
            Logger.LogException(e);
            Logger.LogError($"[{nameof(PlayerPrefsProvider)}::TryDeleteKeyAsync] Error deleting key: {e.Message}");
            return UniTask.FromResult(false);
        }
    }

    public override UniTask<bool> TryDeleteAllAsync(CancellationToken token)
    {
        _cache.Clear();
        PlayerPrefs.DeleteAll();
        return UniTask.FromResult(true);
    }
}
}