using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Formatter;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using UnityEngine;
using VContainer;

namespace CustomUtils.Runtime.StartUp
{
    [PublicAPI]
    public sealed class StartUpController : IStartUpController
    {
        private readonly IObjectResolver _objectResolver;

        internal StartUpController(IObjectResolver objectResolver)
        {
            _objectResolver = objectResolver;
        }

        public async UniTask InitializeSteps(List<StepBase> steps, CancellationToken cancellationToken)
        {
            try
            {
                for (var i = 0; i < steps.Count; i++)
                {
                    steps[i].OnStepCompletedObservable
                        .Subscribe(this, static (stepData, self) => self.LogStepCompletion(stepData))
                        .RegisterTo(cancellationToken);

                    _objectResolver.Inject(steps[i]);
                    await steps[i].ExecuteAsync(i, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void LogStepCompletion(StepData stepData)
        {
            var message = StringFormatter.Format("[StartUpController::LogStepCompletion] Step {0} completed: {1}",
                stepData.Step, stepData.StepName);

            Debug.Log(message);
        }
    }
}