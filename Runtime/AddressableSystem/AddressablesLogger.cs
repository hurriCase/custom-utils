using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace CustomUtils.Runtime.AddressableSystem
{
    internal static class AddressablesLogger
    {
        [Conditional("ADDRESSABLES_LOG_ALL")]
        internal static void Log(string message)
        {
            Debug.Log(message);
        }

        internal static void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }

        internal static void LogError(string message)
        {
            Debug.LogError(message);
        }

        internal static StopWatchScope LogWithTimePast(string message) => new(message);
    }
}