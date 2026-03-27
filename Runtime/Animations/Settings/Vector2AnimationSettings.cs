using CustomUtils.Runtime.Animations.Base.Settings;
using UnityEngine;

namespace CustomUtils.Runtime.Animations.Settings
{
    [CreateAssetMenu(
        fileName = nameof(Vector2AnimationSettings),
        menuName = AnimationSettingsPath + nameof(Vector2AnimationSettings)
    )]
    public sealed class Vector2AnimationSettings : TweenAnimationSettingsBase<Vector2> { }
}