#if PRIMETWEEN_PRO_INSTALLED
using System.Reflection;
using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions
{
    [PublicAPI]
    public static class TweenAnimationExtensions
    {
        // PrimeTween reverses a playing TweenAnimation by negating its sequence time scale, and
        // TweenAnimation.timeScale writes a positive scale back, turning the reversal forward again.
        // The sequence is internal to PrimeTween, so it's read via reflection to keep the sign
        private static readonly FieldInfo _sequenceField =
            typeof(TweenAnimation).GetField("_sequence", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void SetTimeScaleKeepingDirection(this TweenAnimation animation, float timeScale)
        {
            var sequence = (Sequence)_sequenceField.GetValue(animation);
            var isRewinding = sequence is { isAlive: true, timeScale: < 0f };

            animation.timeScale = timeScale;

            if (isRewinding)
                sequence.timeScale = -timeScale;
        }

        /// <summary>
        /// Puts the animation into its start pose, even when it has never played.
        /// </summary>
        public static void ResetToStart(this TweenAnimation animation)
        {
            // Reset leaves an animation that has never played at its current pose; setting the progress of a
            // forward run to zero writes the start values.
            animation.Reset();
            animation.state = true;
            animation.progressTotal = 0f;
        }

        public static void PlayToState(this TweenAnimation animation, bool state, float timeScale)
        {
            animation.SetTimeScaleKeepingDirection(timeScale);
            animation.state = state;
        }

        public static void SetStateInstantly(this TweenAnimation animation, bool state)
        {
            if (state)
            {
                animation.state = true;
                animation.Complete();
                return;
            }

            // A sequence playing backward is inverted, and Reset would move it to its inverted start, the end pose.
            animation.StopAndReset();
        }

        public static void StopAndReset(this TweenAnimation animation)
        {
            if (animation.isAlive)
                animation.Stop();

            animation.Reset();
        }

        /// <summary>
        /// Length of one forward play at time scale 1, nested animation components included.
        /// <see cref="TweenAnimation.durationTotal"/> is known only while the animation plays.
        /// </summary>
        public static float CalculateDuration(this TweenAnimation animation)
        {
            var duration = 0f;
            foreach (var data in animation.animations)
            {
                var target = data.targets is { Count: > 0 } ? data.targets[0] : null;
                var cycles = Mathf.Max(1, data.cycles);
                var length = target is TweenAnimationComponent component
                    ? component.animation.CalculateDuration()
                    : data.duration * cycles;
                var endTime = data.startTime + length;
                duration = Mathf.Max(duration, endTime);
            }

            return duration;
        }
    }
}
#endif
