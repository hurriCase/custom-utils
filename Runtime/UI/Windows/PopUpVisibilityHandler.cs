using CustomUtils.Runtime.Animations.Base;
using CustomUtils.Runtime.Extensions;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Windows
{
    [PublicAPI]
    public class PopUpVisibilityHandler : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeReference, SerializeReferenceDropdown] private IAnimation<VisibilityState> _visibilityAnimation;

        public virtual async UniTask ShowAsync()
        {
            _canvasGroup.Show();

            await _visibilityAnimation.PlayAnimation(VisibilityState.Visible);
        }

        public virtual async UniTask HideAsync()
        {
            await _visibilityAnimation.PlayAnimation(VisibilityState.Hidden);

            _canvasGroup.Hide();
        }

        public void HideImmediately()
        {
            _visibilityAnimation.PlayAnimation(VisibilityState.Hidden, true);

            _canvasGroup.Hide();
        }
    }
}