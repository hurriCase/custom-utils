using System.Collections.Generic;
using CustomUtils.Runtime.Animations.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Windows
{
    [PublicAPI]
    public class VisibilityHandler : MonoBehaviour
    {
        [SerializeReference, SerializeReferenceDropdown] private List<IAnimation<VisibilityState>> _visibilityAnimations;

        public virtual async UniTask ShowAsync() => await CreateVisibilitySequence(VisibilityState.Visible);

        public virtual async UniTask HideAsync() => await CreateVisibilitySequence(VisibilityState.Hidden);

        public void HideImmediately()
        {
            foreach (var animation1 in _visibilityAnimations)
                animation1.PlayAnimation(VisibilityState.Hidden, true);
        }

        private async UniTask CreateVisibilitySequence(VisibilityState visibilityState)
        {
            var sequence = Sequence.Create();
            foreach (var visibilityAnimation in _visibilityAnimations)
            {
                var playAnimation = visibilityAnimation.PlayAnimation(visibilityState);
                sequence.Group(playAnimation);
            }

            await sequence;
        }
    }
}