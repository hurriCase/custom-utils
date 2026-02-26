using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace CustomUtils.Runtime.Storage
{
    internal static class Logger
    {
        [Conditional("STORAGE_LOG_ALL")]
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

        internal static void LogException(Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}