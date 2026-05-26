using CustomUtils.Runtime.Animations.Base.Settings;
using UnityEngine;

namespace CustomUtils.Runtime.Animations.Settings
{
    [CreateAssetMenu(
        fileName = nameof(Vector4AnimationSettings),
        menuName = AnimationSettingsPath + nameof(Vector4AnimationSettings)
    )]
    public sealed class Vector4AnimationSettings : TweenAnimationSettingsBase<Vector4> { }
}