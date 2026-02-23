using System;
using System.Threading;
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
        /// <summary>
        /// Gets an observable that emits when the step completes execution.
        /// </summary>
        public Observable<StepData> OnStepCompletedObservable => _stepCompletedSubject.AsObservable();

        private readonly Subject<StepData> _stepCompletedSubject = new();

        protected virtual string LoadingText { get; }

        protected const string InitializationStepsPath = "Initialization Steps/";

        internal async UniTask ExecuteAsync(int step, CancellationToken token)
        {
            try
            {
                await ExecuteInternalAsync(token);
                _stepCompletedSubject.OnNext(new StepData(step, GetType().Name, LoadingText));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{GetType().Name}::ExecuteAsync] Step initialization failed: {ex.Message}");
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Executes the internal initialization logic for the step.
        /// </summary>
        /// <param name="token">The cancellation token to stop execution.</param>
        /// <returns>A task representing the asynchronous execution operation.</returns>
        protected abstract UniTask ExecuteInternalAsync(CancellationToken token);
    }
}