using System;
using System.Diagnostics;
using Cysharp.Text;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.AddressableSystem
{
    /// <summary>
    /// Disposable struct for measuring and logging operation execution time.
    /// </summary>
    [PublicAPI]
    public readonly struct StopWatchScope : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly string _message;

        /// <summary>
        /// Initializes a new StopWatchScope and starts timing.
        /// </summary>
        /// <param name="message">Message to log with the elapsed time.</param>
        public StopWatchScope(string message)
        {
            _stopwatch = Stopwatch.StartNew();
            _message = message;
        }

        /// <summary>
        /// Stops timing and logs the elapsed time with the message.
        /// </summary>
        public void Dispose()
        {
            _stopwatch.Stop();

            AddressablesLogger.Log(ZString.Format("{0} Operation was done in {1}ms",
                _message,
                _stopwatch.ElapsedMilliseconds));
        }
    }
}