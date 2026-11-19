using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.AnimationSkipping;
using CustomUtils.Runtime.UI.SequentialShow;
using CustomUtils.Runtime.UI.Windows.Windows.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.Windows
{
    /// <summary>
    /// Plays a window's show and hide: its pose, background, content cascade and sounds. Blocks input while the window
    /// is not fully shown.
    /// </summary>
    [PublicAPI]
    public class VisibilityHandler : MonoBehaviour, ISkippableAnimation
    {
        [field: SerializeField] internal WindowMode Mode { get; private set; }
        [field: SerializeField] internal List<Button> CloseButtons { get; private set; }

        [SerializeField] private TweenAnimation _animation;
        [SerializeField] private List<SharedElement> _sharedElements;
        [SerializeField] private TweenAnimationComponent _hideAnimation;
        [SerializeField] private TweenAnimationComponent _backgroundAnimation;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _showSound;
        [SerializeField] private AudioClip _hideSound;
        [SerializeField] private List<CanvasGroup> _interactiveGroups;
        [SerializeField] private SequentialShowAnimation _contentAnimation;
        [SerializeField] private float _contentAnimationDelay;
        [SerializeField] private float _blendHideDuration;

        internal IReadOnlyList<SharedElement> SharedElements => _sharedElements;

        // Handlers on inactive objects are driven before their Awake runs, so the pose is created on first use.
        private WindowPose Pose =>
            _pose ??= new WindowPose(_animation, _hideAnimation, _sharedElements, Mode, _blendHideDuration);

        private WindowPose _pose;
        private bool _isVisible;

        /// <summary>
        /// Plays the show, or jumps to the shown pose when <paramref name="isInstant"/> is true. Completes when it
        /// has finished.
        /// </summary>
        public virtual async UniTask ShowAsync(CancellationToken token, bool isInstant = false)
        {
            await PlayVisibilityAsync(true, isInstant, token);
        }

        /// <summary>
        /// Plays the hide, or jumps to the hidden pose when <paramref name="isInstant"/> is true. Completes when it
        /// has finished.
        /// </summary>
        public virtual async UniTask HideAsync(CancellationToken token, bool isInstant = false)
        {
            await PlayVisibilityAsync(false, isInstant, token);
        }

        /// <summary>
        /// Plays the show when <paramref name="isVisible"/> is true, otherwise the hide.
        /// </summary>
        public virtual async UniTask ToggleVisibilityAsync(
            bool isVisible,
            CancellationToken token,
            bool isInstant = false)
        {
            if (isVisible)
            {
                await ShowAsync(token, isInstant);
                return;
            }

            await HideAsync(token, isInstant);
        }

        /// <summary>
        /// Jumps to the hidden pose and blocks input.
        /// </summary>
        public void HideImmediately()
        {
            PlayAnimation(false, true);
            UpdateInteractivity(false);
        }

        public void SpeedUp(float timeScale)
        {
            Pose.SpeedUp(timeScale, _isVisible);

            if (_backgroundAnimation)
                _backgroundAnimation.animation.SetTimeScaleKeepingDirection(timeScale);
        }

        internal void HoldSharedElements(IEnumerable<string> keys)
        {
            Pose.HoldSharedElements(keys);
        }

        internal void Reveal()
        {
            PlayAnimation(true, true);
            UpdateInteractivity(true);
        }

        internal async UniTask ReturnAsync(CancellationToken token)
        {
            if (_isVisible)
                return;

            if (Pose.IsHideOneWay)
            {
                await PlayVisibilityAsync(true, false, token);
                return;
            }

            _isVisible = true;

            if (_backgroundAnimation)
                _backgroundAnimation.animation.PlayToState(true, 1f);

            if (_contentAnimation)
                _contentAnimation.Return();

            Pose.Return();
            PlaySound(_showSound);

            await WaitUntilPlayedAsync(Pose.ReturningAnimation, true, token);
        }

        internal async UniTask BlendOutAsync(CancellationToken token)
        {
            if (!_isVisible)
                return;

            await PlayVisibilityAsync(false, false, token);
        }

        internal void ShowBackground()
        {
            if (_backgroundAnimation)
                _backgroundAnimation.animation.SetStateInstantly(true);
        }

        internal void HideBackground()
        {
            if (_backgroundAnimation)
                _backgroundAnimation.animation.SetStateInstantly(false);
        }

        internal void FadeOutBackground()
        {
            if (_backgroundAnimation)
                _backgroundAnimation.animation.PlayToState(false, 1f);
        }

        private async UniTask PlayVisibilityAsync(bool isVisible, bool isInstant, CancellationToken token)
        {
            PlayAnimation(isVisible, isInstant);

            var playingAnimation = Pose.GetPlayingAnimation(isVisible);
            await WaitUntilPlayedAsync(playingAnimation, isVisible, token);
        }

        private async UniTask WaitUntilPlayedAsync(
            TweenAnimation playingAnimation,
            bool isVisible,
            CancellationToken token)
        {
            using (SkippableAnimations.Begin(this))
            {
                await playingAnimation.ToYieldInstruction().ToUniTask(cancellationToken: token);

                foreach (var element in _sharedElements)
                    await element.Animation.animation.ToYieldInstruction().ToUniTask(cancellationToken: token);

                if (_backgroundAnimation)
                    await _backgroundAnimation.animation.ToYieldInstruction().ToUniTask(cancellationToken: token);
            }

            if (!isVisible || !_isVisible)
                return;

            UpdateInteractivity(true);
        }

        private void PlayAnimation(bool isVisible, bool isInstant)
        {
            if (!isVisible)
                UpdateInteractivity(false);

            if (isInstant)
            {
                _isVisible = isVisible;

                if (_backgroundAnimation)
                    _backgroundAnimation.animation.SetStateInstantly(isVisible);

                Pose.SetInstantly(isVisible);
                UpdateContentAnimation(isVisible, true);
                return;
            }

            if (_isVisible == isVisible)
                return;

            _isVisible = isVisible;

            if (_backgroundAnimation && isVisible)
                _backgroundAnimation.animation.PlayToState(true, 1f);

            Pose.Play(isVisible);
            UpdateContentAnimation(isVisible, false);

            var sound = isVisible ? _showSound : _hideSound;
            PlaySound(sound);
        }

        private void UpdateContentAnimation(bool isVisible, bool isInstant)
        {
            if (!_contentAnimation)
                return;

            if (isVisible && isInstant)
            {
                _contentAnimation.ShowInstantly();
                return;
            }

            if (isVisible)
            {
                _contentAnimation.Play(_contentAnimationDelay);
                return;
            }

            _contentAnimation.Hide(isInstant);
        }

        private void PlaySound(AudioClip sound)
        {
            if (sound)
                _audioSource.PlayOneShot(sound);
        }

        private void UpdateInteractivity(bool isVisible)
        {
            foreach (var canvasGroup in _interactiveGroups)
            {
                canvasGroup.interactable = isVisible;
                canvasGroup.blocksRaycasts = isVisible;
            }
        }
    }
}
