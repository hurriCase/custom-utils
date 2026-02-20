using System;
using CustomUtils.Runtime.Animations.Base;
using CustomUtils.Runtime.CustomTypes.Collections;
using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.Animations
{
    /// <inheritdoc />
    /// <summary>
    /// Swaps the sprite of an Image component based on state, with optional delay.
    /// </summary>
    /// <typeparam name="TState">The enum type representing animation states.</typeparam>
    [PublicAPI]
    [Serializable]
    public sealed class SpriteSwapAnimation<TState> : IAnimation<TState>
        where TState : unmanaged, Enum
    {
        [SerializeField] private Image _target;
        [SerializeField] private float _duration;
        [SerializeField] private EnumArray<TState, Sprite> _states;

        private Tween _currentAnimation;

        private Sprite _targetSprite;

        public Tween PlayAnimation(TState state, bool isInstant)
        {
            _targetSprite = _states[state];

            if (isInstant)
            {
                UpdateSprite();
                return default;
            }

            if (_currentAnimation.isAlive)
                _currentAnimation.Stop();

            return _currentAnimation = Tween.Delay(this, _duration, static self => self.UpdateSprite());
        }

        private void UpdateSprite()
        {
            _target.sprite = _targetSprite;
        }

        public void CancelAnimation()
        {
            _currentAnimation.Stop();
        }
    }
}