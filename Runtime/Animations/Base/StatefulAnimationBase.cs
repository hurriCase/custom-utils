using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Animations.Base
{
    [Serializable]
    public abstract class StatefulAnimationBase<TState> : IAnimation<TState>
        where TState : unmanaged, Enum
    {
        [SerializeField] private bool _skipIfSameState;

        [SerializeField] private TState _currentState;

        protected Tween CurrentAnimation { get; private set; }

        private bool _hasState;

        public Tween PlayAnimation(TState state, bool isInstant = false)
        {
            if (_skipIfSameState && _hasState && EqualityComparer<TState>.Default.Equals(_currentState, state))
                return CurrentAnimation;

            _hasState = true;
            _currentState = state;

            return CurrentAnimation = OnPlayAnimation(state, isInstant);
        }

        public void CancelAnimation()
        {
            CurrentAnimation.Stop();
        }

        protected abstract Tween OnPlayAnimation(TState state, bool isInstant);
    }
}