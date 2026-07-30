#if PRIMETWEEN_INSTALLED
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Animations.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.Windows
{
    [PublicAPI]
    public class VisibilityHandler : MonoBehaviour
    {
        [field: SerializeField] internal Button CloseButton { get; private set; }

        [SerializeReference, SerializeReferenceDropdown] private List<IAnimation<VisibilityState>> _visibilityAnimations;

        private List<UniTask> _cachedTasks = new();

        public virtual async UniTask ShowAsync(CancellationToken token, bool isInstant)
            => await CreateVisibilitySequence(VisibilityState.Visible, token, isInstant);

        public virtual async UniTask HideAsync(CancellationToken token, bool isInstant)
            => await CreateVisibilitySequence(VisibilityState.Hidden, token, isInstant);

        public void HideImmediately()
        {
            foreach (var visibilityAnimation in _visibilityAnimations)
                visibilityAnimation.PlayAnimation(VisibilityState.Hidden, true);
        }

        private async UniTask CreateVisibilitySequence(VisibilityState visibilityState, CancellationToken token, bool isInstant)
        {
            _cachedTasks.Clear();

            foreach (var visibilityAnimation in _visibilityAnimations)
            {
                // ToYieldInstruction() is required to avoid struct boxing allocation.
                // It uses a pooled TweenCoroutineEnumerator instead of allocating on each call.
                _cachedTasks.Add(visibilityAnimation.PlayAnimation(visibilityState, isInstant)
                    .ToYieldInstruction()
                    .ToUniTask(cancellationToken: token));
            }

            await UniTask.WhenAll(_cachedTasks);
        }
    }
}
#endif