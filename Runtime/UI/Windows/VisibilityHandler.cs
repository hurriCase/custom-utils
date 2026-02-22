using System.Collections.Generic;
using CustomUtils.Runtime.Animations.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Windows
{
    [PublicAPI]
    public class VisibilityHandler : MonoBehaviour
    {
        [SerializeReference, SerializeReferenceDropdown] private List<IAnimation<VisibilityState>> _visibilityAnimations;

        private List<UniTask> _cachedTasks = new();

        public virtual async UniTask ShowAsync() => await CreateVisibilitySequence(VisibilityState.Visible);

        public virtual async UniTask HideAsync() => await CreateVisibilitySequence(VisibilityState.Hidden);

        public void HideImmediately()
        {
            foreach (var visibilityAnimation in _visibilityAnimations)
                visibilityAnimation.PlayAnimation(VisibilityState.Hidden, true);
        }

        private async UniTask CreateVisibilitySequence(VisibilityState visibilityState)
        {
            _cachedTasks.Clear();

            foreach (var visibilityAnimation in _visibilityAnimations)
            {
                // ToYieldInstruction() is required to avoid struct boxing allocation.
                // It uses a pooled TweenCoroutineEnumerator instead of allocating on each call.
                _cachedTasks.Add(visibilityAnimation.PlayAnimation(visibilityState)
                    .ToYieldInstruction()
                    .ToUniTask());
            }

            await UniTask.WhenAll(_cachedTasks);
        }
    }
}