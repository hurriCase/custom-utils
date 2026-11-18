#if PRIMETWEEN_PRO_INSTALLED
using System.Threading;
using CustomUtils.Runtime.Extensions;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using PrimeTween;
using R3;
using UnityEngine;

namespace CustomUtils.Runtime.UI.SequentialShow
{
    /// <summary>
    /// One item of a <see cref="SequentialShowAnimation"/>: plays its show when its row comes and its hide when the
    /// cascade hides. An item nested inside another item moves with it and is not an item of its own.
    /// </summary>
    [PublicAPI]
    public sealed class TweenSequentialShowItem : MonoBehaviour
    {
        [SerializeField] private TweenAnimationComponent _showAnimation;
        [SerializeField] private TweenAnimationComponent _hideAnimation;

        public Observable<Unit> OnShown => _shown;
        private readonly Subject<Unit> _shown = new();

        /// <summary>
        /// Puts the item into its visible pose at once.
        /// </summary>
        public void ShowInstantly()
        {
            ResetHide();

            var showAnimation = _showAnimation.animation;
            showAnimation.state = true;
            showAnimation.Complete();
            _shown.OnNext(Unit.Default);
        }

        internal void ResetToStart()
        {
            ResetHide();
            _showAnimation.animation.ResetToStart();
        }

        internal void Show(float timeScale)
        {
            var showAnimation = _showAnimation.animation;
            showAnimation.state = true;
            showAnimation.SetTimeScaleKeepingDirection(timeScale);
            _shown.OnNext(Unit.Default);
        }

        internal void Hide(bool isReversingShow)
        {
            if (isReversingShow)
            {
                _showAnimation.animation.state = false;
                return;
            }

            if (!_hideAnimation)
                return;

            var hideAnimation = _hideAnimation.animation;
            hideAnimation.Reset();
            hideAnimation.state = true;
        }

        internal void Return(bool isReversingShow)
        {
            if (isReversingShow)
            {
                _showAnimation.animation.state = true;
                return;
            }

            if (_hideAnimation)
                _hideAnimation.animation.state = false;
        }

        internal void SpeedUp(float timeScale)
        {
            _showAnimation.animation.SetTimeScaleKeepingDirection(timeScale);

            if (_hideAnimation)
                _hideAnimation.animation.SetTimeScaleKeepingDirection(timeScale);
        }

        public async UniTask WaitUntilShownAsync(CancellationToken token)
        {
            await _showAnimation.animation.ToYieldInstruction().ToUniTask(cancellationToken: token);
        }

        private void ResetHide()
        {
            if (_hideAnimation)
                _hideAnimation.animation.StopAndReset();
        }

        private void OnDestroy()
        {
            _shown.Dispose();
        }
    }
}
#endif
