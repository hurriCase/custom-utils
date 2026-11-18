#if PRIMETWEEN_INSTALLED
using System.Collections.Generic;
using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Effects
{
    /// <inheritdoc />
    /// <summary>
    /// Presses a selectable into the shadow of its images: moves every child toward the first shadow
    /// and shrinks each shadow offset by the same amount, so the shadows stay in place.
    /// Shadows come from procedural images and from sprite images with a <see cref="Shadow"/> effect.
    /// Without listed shadows it takes the children's procedural images that cast a shadow,
    /// and disables itself when there are none.
    /// </summary>
    [PublicAPI]
    [AddComponentMenu("UI/Shadow Press Animation")]
    public sealed class ShadowPressAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Selectable _selectable;
        [SerializeField] private List<ProceduralImage> _shadowImages;
        [SerializeField] private List<Shadow> _shadowEffects;
        [SerializeField, Range(0f, 1f)] private float _pressDepth;
        [SerializeField] private TweenSettings _pressSettings;
        [SerializeField] private TweenSettings _releaseSettings;

        private readonly List<RectTransform> _children = new();
        private readonly List<Vector2> _restPositions = new();
        private readonly List<ProceduralShadow> _restShadows = new();
        private readonly List<Vector2> _restEffectDistances = new();
        private Vector2 _pressDirection;
        private float _pressProgress;
        private Tween _tween;

        private void Awake()
        {
            var findShadowImages = _shadowImages.Count == 0 && _shadowEffects.Count == 0;
            foreach (RectTransform child in transform)
            {
                _children.Add(child);
                _restPositions.Add(child.anchoredPosition);

                if (!findShadowImages)
                    continue;

                var hasImage = child.TryGetComponent(out ProceduralImage image);
                if (hasImage && image.Shadow.IsEnabled)
                    _shadowImages.Add(image);
            }

            if (_shadowImages.Count == 0 && _shadowEffects.Count == 0)
            {
                enabled = false;
                return;
            }

            foreach (var shadowImage in _shadowImages)
                _restShadows.Add(shadowImage.Shadow);

            foreach (var shadowEffect in _shadowEffects)
                _restEffectDistances.Add(shadowEffect.effectDistance);

            _pressDirection = _restShadows.Count > 0 ? _restShadows[0].Offset : _restEffectDistances[0];
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_selectable.IsInteractable())
                AnimatePress(1f, _pressSettings);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            AnimatePress(0f, _releaseSettings);
        }

        private void AnimatePress(float targetProgress, TweenSettings settings)
        {
            _tween.Stop();
            _tween = Tween.Custom(this, _pressProgress, targetProgress, settings,
                static (self, progress) => self.ApplyPress(progress));
        }

        private void ApplyPress(float progress)
        {
            _pressProgress = progress;

            var depth = _pressDepth * progress;
            var pressOffset = _pressDirection * depth;
            for (var i = 0; i < _children.Count; i++)
                _children[i].anchoredPosition = _restPositions[i] + pressOffset;

            for (var i = 0; i < _shadowImages.Count; i++)
            {
                var restShadow = _restShadows[i];
                var shadowOffset = restShadow.Offset - pressOffset;
                var shadow = new ProceduralShadow(restShadow.IsEnabled, shadowOffset, restShadow.Color);
                _shadowImages[i].SetShadow(shadow);
            }

            for (var i = 0; i < _shadowEffects.Count; i++)
                _shadowEffects[i].effectDistance = _restEffectDistances[i] - pressOffset;
        }

        private void OnDisable()
        {
            _tween.Stop();

            if (_pressProgress > 0f)
                ApplyPress(0f);
        }
    }
}
#endif