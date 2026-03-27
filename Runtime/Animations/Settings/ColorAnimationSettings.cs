using CustomUtils.Runtime.Animations.Base.Settings;
using UnityEngine;

namespace CustomUtils.Runtime.Animations.Settings
{
    [CreateAssetMenu(
        fileName = nameof(ColorAnimationSettings),
        menuName = AnimationSettingsPath + nameof(ColorAnimationSettings)
    )]
    public sealed class ColorAnimationSettings : TweenAnimationSettingsBase<Color> { }
}