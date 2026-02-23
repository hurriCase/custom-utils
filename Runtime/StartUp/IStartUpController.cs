using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.StartUp
{
    /// <summary>
    /// Defines a controller for managing startup initialization steps.
    /// </summary>
    [PublicAPI]
    public interface IStartUpController
    {
        /// <summary>
        /// Initializes the specified startup steps in sequence.
        /// </summary>
        /// <param name="steps">The list of steps to initialize.</param>
        /// <param name="token">The cancellation token to stop initialization.</param>
        /// <returns>A task representing the asynchronous initialization operation.</returns>
        UniTask InitializeSteps(List<StepBase> steps, CancellationToken token);
    }
}