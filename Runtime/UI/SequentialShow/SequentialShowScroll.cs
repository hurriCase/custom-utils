#if PRIMETWEEN_PRO_INSTALLED
using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.SequentialShow
{
    /// <summary>
    /// Where a <see cref="SequentialShowAnimation"/> scrolls its scroll view: each cascade starts the scroll view at
    /// its first item and moves it to the item or position set here while the items appear. Without a destination
    /// the scroll view stays at the start. Dragging the scroll view stops the scroll.
    /// </summary>
    [PublicAPI]
    public sealed class SequentialShowScroll : MonoBehaviour, IBeginDragHandler
    {
        [SerializeField] private float _duration;
        [SerializeField] private Ease _ease;

        private RectTransform _targetItem;
        private float _targetNormalizedPosition;
        private bool _hasDestination;
        private Tween _tween;

        /// <summary>
        /// Makes the next cascades end their scroll with <paramref name="item"/> centred, as far as the content allows.
        /// </summary>
        public void ScrollTo(RectTransform item)
        {
            _targetItem = item;
            _hasDestination = true;
        }

        /// <summary>
        /// Makes the next cascades end their scroll at <paramref name="normalizedPosition"/> along the scroll view's
        /// axis, the same value as <see cref="ScrollRect.normalizedPosition"/>.
        /// </summary>
        public void ScrollTo(float normalizedPosition)
        {
            _targetItem = null;
            _targetNormalizedPosition = normalizedPosition;
            _hasDestination = true;
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _tween.Stop();
        }

        internal Vector2 GetOffset(ScrollRect scrollRect)
        {
            if (!_hasDestination)
                return Vector2.zero;

            var viewport = scrollRect.GetViewport();
            var content = scrollRect.content;
            var viewportRect = viewport.rect;
            var contentRect = content.rect;
            var contentScale = (Vector2)content.localScale;
            var contentPosition = (Vector2)content.localPosition;
            var scaledContentMin = Vector2.Scale(contentRect.min, contentScale);
            var scaledContentMax = Vector2.Scale(contentRect.max, contentScale);
            var contentMin = contentPosition + scaledContentMin;
            var contentMax = contentPosition + scaledContentMax;
            var minOffset = viewportRect.max - contentMax;
            var maxOffset = viewportRect.min - contentMin;
            var destinationOffset = GetDestinationOffset(viewport, minOffset, maxOffset);

            var offsetX = scrollRect.horizontal ? ClampOffset(destinationOffset.x, minOffset.x, maxOffset.x) : 0f;
            var offsetY = scrollRect.vertical ? ClampOffset(destinationOffset.y, minOffset.y, maxOffset.y) : 0f;
            return new Vector2(offsetX, offsetY);
        }

        internal float FitItemInterval(float itemInterval, int lastStep)
            => lastStep > 0 ? Mathf.Min(itemInterval, _duration / lastStep) : itemInterval;

        internal void Play(RectTransform content, Vector2 offset, float timeScale)
        {
            if (offset == Vector2.zero)
                return;

            var endPosition = content.anchoredPosition + offset;
            _tween = Tween.UIAnchoredPosition(content, endPosition, _duration, _ease, useUnscaledTime: true);
            _tween.timeScale = timeScale;
        }

        internal void SpeedUp(float timeScale)
        {
            if (_tween.isAlive)
                _tween.timeScale = timeScale;
        }

        internal void Complete()
        {
            _tween.Complete();
        }

        internal void Stop()
        {
            _tween.Stop();
        }

        // Normalized position 0 shows the content's min edge (bottom, left), which is the largest offset.
        private Vector2 GetDestinationOffset(RectTransform viewport, Vector2 minOffset, Vector2 maxOffset)
        {
            if (!_targetItem)
                return Vector2.LerpUnclamped(maxOffset, minOffset, _targetNormalizedPosition);

            var targetPosition = _targetItem.GetLocalPositionIn(viewport);
            var targetCenter = targetPosition + _targetItem.rect.center;
            var viewportCenter = viewport.rect.center;
            return viewportCenter - targetCenter;
        }

        private float ClampOffset(float offset, float minOffset, float maxOffset)
        {
            if (minOffset > maxOffset)
                return 0f;

            return Mathf.Clamp(offset, minOffset, maxOffset);
        }

        private void OnDestroy()
        {
            _tween.Stop();
        }
    }
}
#endif
