using System;
using CustomUtils.Runtime.Animations.Base;
using CustomUtils.Runtime.Animations.Settings;
using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Animations
{
    /// <summary>
    /// Animates the Z-axis rotation of a RectTransform based on state, accounting for angle wrapping.
    /// </summary>
    /// <typeparam name="TState">The enum type representing animation states.</typeparam>
    [PublicAPI]
    [Serializable]
    public sealed class RotationZAnimation<TState> : AnimationBase<TState, float, FloatAnimationSettings>
        where TState : unmanaged, Enum
    {
        [SerializeField] private RectTransform _target;

        private float _currentRotationZ;

        protected override void SetValueInstant(float value)
        {
            var endValue = CalculateFinalZ(value);
            SetRotation(endValue);
        }

        protected override Tween CreateTween(FloatAnimationSettings animationSettings)
            => Tween.Custom(this,
                _currentRotationZ,
                CalculateFinalZ(animationSettings.Value),
                animationSettings.TweenSettings, static (self, rotationZ) => self.SetRotation(rotationZ));

        private void SetRotation(float rotationZ)
        {
            var targetTransform = _target.transform;
            var euler = targetTransform.eulerAngles;
            euler.z = rotationZ;
            targetTransform.eulerAngles = euler;

            _currentRotationZ = rotationZ;
        }

        private float CalculateFinalZ(float targetZ)
        {
            var deltaZ = Mathf.DeltaAngle(_currentRotationZ, targetZ);
            return _currentRotationZ + deltaZ;
        }
    }
}