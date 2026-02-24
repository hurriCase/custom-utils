using System;
using System.Threading;
using CustomUtils.Runtime.LocalizationProviders;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using UnityEngine;

namespace CustomUtils.Runtime.StartUp
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for startup initialization steps.
    /// </summary>
    [PublicAPI]
    public abstract class StepBase : ScriptableObject
    {
        [SerializeReference, SerializeReferenceDropdown] private ILocalization _loadingKey;

        /// <summary>
        /// Gets an observable that emits when the step completes execution.
        /// </summary>
        public Observable<string> OnStepCompletedObservable => _stepCompletedSubject;

        protected const string InitializationStepsPath = "Initialization Steps/";

        private readonly Subject<string> _stepCompletedSubject = new();

        public virtual async UniTask<bool> ExecuteAsync(CancellationToken token)
        {
            try
            {
                var isSuccess = await ExecuteInternalAsync(token);

                var loadingText = await _loadingKey.GetLocalizationAsync(token);
                _stepCompletedSubject.OnNext(loadingText);

                LogStep(isSuccess);
                return isSuccess;
            }
            catch (Exception ex)
            {
                LogStep(false, ex.Message);
                Debug.LogException(ex);
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
