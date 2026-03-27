using System;
using CustomUtils.Runtime.CustomTypes.Collections;
using UnityEngine;

namespace CustomUtils.Runtime.Animations.Base.Settings
{
    public class DelayedAnimationSettingsBase<TState, TContent> : AnimationSettingsBase
        where TState : unmanaged, Enum
    {
        [field: SerializeField] internal float Delay { get; private set; }
        [field: SerializeField] internal EnumArray<TState, TContent> States { get; private set; }
    }
}