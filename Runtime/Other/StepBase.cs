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

        /// <summary>
        /// Gets the step progress in the 0..1 range. Resets to 0 when the step starts and becomes 1 when it ends.
        /// </summary>
        public ReadOnlyReactiveProperty<float> Progress => _progress;

        protected const string InitializationStepsPath = "Initialization Steps/";

        private readonly Subject<string> _stepStarted = new();
        private readonly ReactiveProperty<float> _progress = new();

        public virtual async UniTask<bool> ExecuteAsync(CancellationToken token)
        {
            _progress.Value = 0f;

            try
            {
                if (_loadingKey != null)
                {
                    var loadingText = await _loadingKey.GetLocalizationAsync(token).SuppressAsync(token);
                    _stepStarted.OnNext(loadingText.Data);
                }

                _stepStarted.OnNext(string.Empty);

                var isSuccess = await ExecuteInternalAsync(token);
                _progress.Value = 1f;

                LogStep(isSuccess);
                return isSuccess;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _progress.Value = 1f;

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

        /// <summary>
        /// Reports intermediate progress of the step in the 0..1 range.
        /// </summary>
        /// <param name="progress">The step progress.</param>
        protected void ReportProgress(float progress)
        {
            _progress.Value = progress;
        }

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
