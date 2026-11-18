using System.Collections.Generic;
using CustomUtils.Runtime.Other;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.AnimationSkipping
{
    [PublicAPI]
    public static class SkippableAnimations
    {
        private static readonly List<ISkippableAnimation> _playingAnimations = new();

        static SkippableAnimations()
        {
            StaticResetter.RegisterResetAction(static () => _playingAnimations.Clear());
        }

        public static SkippableAnimationScope Begin(ISkippableAnimation animation)
        {
            _playingAnimations.Add(animation);
            animation.SpeedUp(1f);
            return new SkippableAnimationScope(animation);
        }

        public static void SkipPlaying(float timeScale)
        {
            foreach (var animation in _playingAnimations)
                animation.SpeedUp(timeScale);
        }

        internal static void End(ISkippableAnimation animation)
        {
            _playingAnimations.Remove(animation);
        }
    }
}
