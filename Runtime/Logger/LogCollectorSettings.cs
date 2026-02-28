using CustomUtils.Runtime.AssetLoader;
using CustomUtils.Runtime.CustomTypes.Singletons;
using CustomUtils.Runtime.Other;
using UnityEngine;

namespace CustomUtils.Runtime.Logger
{
    /// <inheritdoc />
    /// <summary>
    /// ScriptableObject that stores configuration settings for the LogCollector.
    /// </summary>
    [Resource(
        ResourcePaths.LoggerSettingsFullPath,
        ResourcePaths.LoggerSettingsAssetName,
        ResourcePaths.LoggerSettingsResourcesPath
    )]
    internal sealed class LogCollectorSettings : SingletonScriptableObject<LogCollectorSettings>
    {
        [field: SerializeField] internal int MaxLogEntries { get; private set; } = 500;
        [field: SerializeField] internal bool CaptureStackTrace { get; private set; } = true;
        [field: SerializeField] internal bool AutoStartCollect { get; private set; } = true;
        [field: SerializeField] internal LogType MinimumLogLevel { get; private set; } = LogType.Warning;
    }
}