using System;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.AnimationSkipping
{
    [PublicAPI]
    public readonly struct SkippableAnimationScope : IDisposable
    {
        private readonly ISkippableAnimation _animation;

        internal SkippableAnimationScope(ISkippableAnimation animation)
        {
            _animation = animation;
        }

        public void Dispose()
        {
            SkippableAnimations.End(_animation);
        }
    }
}
