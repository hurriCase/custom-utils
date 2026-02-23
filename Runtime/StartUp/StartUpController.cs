using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
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

        public async UniTask InitializeSteps(List<StepBase> steps, CancellationToken token)
        {
            foreach (var step in steps)
            {
                _objectResolver.Inject(step);
                await step.ExecuteAsync(token);
            }
        }
    }
}