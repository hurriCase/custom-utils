using System;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Logger
{
    /// <summary>
    /// The LogCollector provides a way to capture and store Unity logs in scenarios where
    /// the standard Unity console is not accessible, such as in release builds or on mobile devices.
    /// </summary>
    [PublicAPI]
    public static class LogCollector
    {
        private static int _maxLogEntries;
        private static bool _captureStackTrace;
        private static LogType _minimumLogLevel;
        private static LogEntry[] _logBuffer;
        private static int _currentIndex;
        private static int _logCount;
        private static bool _isCollecting;

        private static readonly StringBuilder _logBuilder = new(16384);

        private static volatile bool _collectLogs;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            _maxLogEntries = 0;
            _captureStackTrace = false;
            _minimumLogLevel = LogType.Error;
            _logBuffer = Array.Empty<LogEntry>();
            _currentIndex = 0;
            _logCount = 0;
            _isCollecting = false;

            Application.logMessageReceived -= HandleLog;
        }

        /// <summary>
        /// Initializes the log collector with specified parameters from LogCollectorSettings.
        /// </summary>
        public static void Initialize()
        {
            StopCollection();

            _maxLogEntries = LogCollectorSettings.Instance.MaxLogEntries;
            _captureStackTrace = LogCollectorSettings.Instance.CaptureStackTrace;
            _minimumLogLevel = LogCollectorSettings.Instance.MinimumLogLevel;
            _logBuffer = new LogEntry[_maxLogEntries];

            if (LogCollectorSettings.Instance.AutoStartCollect)
                StartCollection();
        }

        /// <summary>
        /// Starts collecting log messages from Unity's logging system.
        /// </summary>
        /// <remarks>
        /// If collection is already in progress, this method has no effect.
        /// </remarks>
        public static void StartCollection()
        {
            if (_isCollecting)
                return;

            // Placed here to prevent this log from being collected
            Application.logMessageReceived += HandleLog;

            _isCollecting = true;
            _collectLogs = true;
        }

        /// <summary>
        /// Stops the collection of log messages.
        /// </summary>
        /// <remarks>
        /// If collection is not in progress, this method has no effect.
        /// </remarks>
        public static void StopCollection()
        {
            if (_isCollecting is false)
                return;

            Application.logMessageReceived -= HandleLog;

            _isCollecting = false;
            _collectLogs = false;

            Debug.Log("[LogCollector::StopCollection] Log collection stopped");
        }

        /// <summary>
        /// Copies all collected logs to the system clipboard.
        /// </summary>
        /// <remarks>
        /// Temporarily stops collection while copying logs if collection is in progress.
        /// Includes device information, operating system, and application version.
        /// Restores the collection state when complete.
        /// </remarks>
        public static void CopyLogs()
        {
            var wasCollecting = _isCollecting;
            if (wasCollecting)
                StopCollection();

            _logBuilder.Length = 0;

            _logBuilder.Append("DEVICE: ").Append(SystemInfo.deviceModel)
                .Append(" | OS: ").Append(SystemInfo.operatingSystem)
                .Append(" | APP: ").Append(Application.version)
                .Append(" | MEMORY: ").Append(SystemInfo.systemMemorySize)
                .AppendLine();

            if (_logCount > 0)
            {
                var startIndex = (_currentIndex - _logCount + 1 + _maxLogEntries) % _maxLogEntries;

                for (var i = 0; i < _logCount; i++)
                {
                    var index = (startIndex + i) % _maxLogEntries;
                    var entry = _logBuffer[index];

                    _logBuilder.Append('[').Append(entry.Timestamp).Append("] ")
                        .Append('[').Append(entry.Type).Append("] ")
                        .AppendLine(entry.Message);

                    if (_captureStackTrace && string.IsNullOrEmpty(entry.StackTrace) is false)
                        _logBuilder.AppendLine(entry.StackTrace);
                }
            }
            else
                _logBuilder.AppendLine("No logs collected.");

            GUIUtility.systemCopyBuffer = _logBuilder.ToString();

            if (wasCollecting)
                StartCollection();
        }

        /// <summary>
        /// Clears all stored log entries.
        /// </summary>
        /// <remarks>
        /// Resets the internal log counter and index.
        /// </remarks>
        public static void ClearLogs()
        {
            _currentIndex = 0;
            _logCount = 0;

            Debug.Log("[LogCollector::ClearLogs] Logs cleared");
        }

        private static void HandleLog(string message, string stackTrace, LogType type)
        {
            if (_collectLogs is false || (int)type > (int)_minimumLogLevel)
                return;

            var index = Interlocked.Increment(ref _currentIndex) % _maxLogEntries;

            _logBuffer[index].Message = message;
            _logBuffer[index].Type = type;
            _logBuffer[index].StackTrace = _captureStackTrace ? stackTrace : string.Empty;
            _logBuffer[index].Timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

            _logCount = Mathf.Min(_logCount + 1, _maxLogEntries);
        }
    }
}