using System;
using CustomUtils.Collections.Scripts;
using UnityEngine;

namespace CustomUtils.Runtime.Animations.Base.Settings
{
    public sealed class DelayedAnimationSettingsBase<TState, TContent> : AnimationSettingsBase
        where TState : unmanaged, Enum
    {
        [field: SerializeField] internal float Delay { get; private set; }
        [field: SerializeField] internal EnumArray<TState, TContent> States { get; private set; }
        [field: SerializeField] internal bool SkipWhenInstant { get; private set; }
        [field: SerializeField] internal bool UseUnscaledTime { get; private set; }
    }
}