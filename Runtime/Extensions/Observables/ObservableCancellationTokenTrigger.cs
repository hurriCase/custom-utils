using System.Threading;
using JetBrains.Annotations;
using R3.Triggers;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions.Observables
{
    [PublicAPI]
    [DisallowMultipleComponent]
    public sealed class ObservableDisableTrigger : ObservableTriggerBase
    {
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>This function is called when the behaviour becomes disabled or inactive.</summary>
        private void OnDisable()
        {
            _cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Returns a CancellationToken that will be cancelled when OnDisable is called.
        /// </summary>
        public CancellationToken GetCancellationToken()
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource = new CancellationTokenSource();

            return _cancellationTokenSource.Token;
        }

        protected override void RaiseOnCompletedOnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}