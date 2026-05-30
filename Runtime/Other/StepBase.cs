using System;
using System.Threading;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.LocalizationProviders;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using UnityEngine;

namespace CustomUtils.Runtime.Other
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for startup initialization steps.
    /// </summary>
    [PublicAPI]
    public abstract class StepBase : ScriptableObject
    {
        [SerializeReference]
#if SERIALIZE_REFERENCE_DROPDOWN_INSTALLED
        [SerializeReferenceDropdown]
#endif
        private ILocalization _loadingKey;

        /// <summary>
        /// Gets an observable that emits when the step completes execution.
        /// </summary>
        public Observable<string> OnStepStarted => _stepStarted;

        protected const string InitializationStepsPath = "Initialization Steps/";

        private readonly Subject<string> _stepStarted = new();

        public virtual async UniTask<bool> ExecuteAsync(CancellationToken token)
        {
            try
            {
                if (_loadingKey != null)
                {
                    var loadingText = await _loadingKey.GetLocalizationAsync(token).SuppressAsync(token);
                    _stepStarted.OnNext(loadingText.Data);
                }

                _stepStarted.OnNext(string.Empty);

                var isSuccess = await ExecuteInternalAsync(token);

                LogStep(isSuccess);
                return isSuccess;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                LogStep(false, exception.Message);
                Debug.LogException(exception);
                return false;
            }
        }

        /// <summary>
        /// Executes the internal initialization logic for the step.
        /// </summary>
        /// <param name="token">The cancellation token to stop execution.</param>
        /// <returns>A task representing the asynchronous execution operation.</returns>
        protected abstract UniTask<bool> ExecuteInternalAsync(CancellationToken token);

        private void LogStep(bool isSuccess, string message = null)
        {
            if (isSuccess)
            {
                Debug.Log($"[{GetType().Name}::LogStep] Step {GetType().Name} completed");
                return;
            }

            Debug.LogError($"[{GetType().Name}::LogStep] " +
                           $"Step initialization failed {(string.IsNullOrEmpty(message) ? "" : message)}");
        }
    }
}
