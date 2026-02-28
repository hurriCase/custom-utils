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
        private bool _isInitialized;

        protected override void SetValueInstant(float value)
        {
            EnsureInitialized();

            var deltaZ = Mathf.DeltaAngle(_currentRotationZ, value);
            SetRotation(_currentRotationZ + deltaZ);
        }

        protected override Tween CreateTween(FloatAnimationSettings animationSettings)
        {
            EnsureInitialized();

            return Tween.Custom(
                this,
                RecalculateFinalZ(animationSettings.TweenSettings),
                static (self, rotationZ) => self.SetRotation(rotationZ));
        }

        private void SetRotation(float rotationZ)
        {
            var targetTransform = _target.transform;
            var euler = targetTransform.eulerAngles;
            euler.z = rotationZ;
            targetTransform.eulerAngles = euler;

            _currentRotationZ = rotationZ;
        }

        private TweenSettings<float> RecalculateFinalZ(TweenSettings<float> tweenSettings)
        {
            var deltaZ = Mathf.DeltaAngle(_currentRotationZ, tweenSettings.endValue);
            tweenSettings.endValue = _currentRotationZ + deltaZ;
            return tweenSettings;
        }

        private void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            _currentRotationZ = _target.eulerAngles.z;
            _isInitialized = true;
        }
    }
}