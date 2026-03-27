using CustomUtils.Runtime.Animations.Base.Settings;
using UnityEngine;

namespace CustomUtils.Runtime.Animations.Settings
{
    [CreateAssetMenu(
        fileName = nameof(FloatAnimationSettings),
        menuName = AnimationSettingsPath + nameof(FloatAnimationSettings)
    )]
    public sealed class FloatAnimationSettings : TweenAnimationSettingsBase<float> { }
}