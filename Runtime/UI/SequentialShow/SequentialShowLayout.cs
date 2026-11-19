#if PRIMETWEEN_PRO_INSTALLED
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.SequentialShow
{
    internal sealed class SequentialShowLayout
    {
        internal IReadOnlyList<SequentialShowItem> Items => _items;

        private readonly List<TweenSequentialShowItem> _foundItems = new();
        private readonly List<SequentialShowAnimation> _foundNestedAnimations = new();
        private readonly List<SequentialShowItem> _items = new();

        internal void Collect(
            SequentialShowAnimation owner,
            SequentialShowOrder order,
            bool isBottomToTop,
            ScrollRect scrollRect,
            Vector2 scrollOffset)
        {
            _items.Clear();

            owner.GetComponentsInChildren(_foundItems);
            foreach (var foundItem in _foundItems)
                TryAddItem(owner, foundItem.transform, foundItem, null, scrollRect, scrollOffset);

            owner.GetComponentsInChildren(_foundNestedAnimations);
            foreach (var nestedAnimation in _foundNestedAnimations)
                TryAddItem(owner, nestedAnimation.transform, null, nestedAnimation, scrollRect, scrollOffset);

            _items.Sort();

            if (isBottomToTop)
                _items.Reverse();

            AssignSteps(order);
        }

        // ScrollRect.normalizedPosition goes through world matrices, which collapse while an ancestor is scaled to
        // zero (a window expanding from scale 0), so the content is aligned to the viewport's start corner locally.
        internal void ResetScroll(ScrollRect scrollRect, bool isBottomToTop)
        {
            if (!scrollRect)
                return;

            scrollRect.StopMovement();

            var viewport = scrollRect.GetViewport();
            var content = scrollRect.content;
            var viewportRect = viewport.rect;
            var contentRect = content.rect;
            var contentScale = content.localScale;
            var viewportStartY = isBottomToTop ? viewportRect.yMin : viewportRect.yMax;
            var contentStartY = isBottomToTop ? contentRect.yMin : contentRect.yMax;
            var viewportStart = new Vector2(viewportRect.xMin, viewportStartY);
            var contentStartOffset = new Vector2(contentRect.xMin * contentScale.x, contentStartY * contentScale.y);
            var contentStart = (Vector2)content.localPosition + contentStartOffset;
            var startOffset = viewportStart - contentStart;
            var offsetX = scrollRect.horizontal ? startOffset.x : 0f;
            var offsetY = scrollRect.vertical ? startOffset.y : 0f;
            var offset = new Vector2(offsetX, offsetY);
            content.anchoredPosition += offset;
        }

        private void TryAddItem(
            SequentialShowAnimation owner,
            Transform itemTransform,
            TweenSequentialShowItem item,
            SequentialShowAnimation nestedAnimation,
            ScrollRect ownerScrollRect,
            Vector2 scrollOffset)
        {
            if (!IsOwnItem(owner, itemTransform))
                return;

            var position = itemTransform.GetLocalPositionIn(owner.transform);
            var isInViewport = IsInViewport(itemTransform, ownerScrollRect, scrollOffset);
            var sequentialShowItem = new SequentialShowItem(item, nestedAnimation, position, isInViewport, 0);
            _items.Add(sequentialShowItem);
        }

        private bool IsOwnItem(SequentialShowAnimation owner, Transform itemTransform)
        {
            var parent = itemTransform.parent;
            var parentOwner = parent.GetComponentInParent<SequentialShowAnimation>();
            var parentItem = parent.GetComponentInParent<TweenSequentialShowItem>();
            var isInsideItem = parentItem && parentItem.transform.IsChildOf(owner.transform);
            return parentOwner == owner && !isInsideItem;
        }

        // The cascade's own scroll moves the content by scrollOffset, so an item counts as visible when the viewport
        // shows it at any point of that scroll.
        private bool IsInViewport(Transform itemTransform, ScrollRect ownerScrollRect, Vector2 scrollOffset)
        {
            var scrollRect = itemTransform.GetComponentInParent<ScrollRect>();
            if (!scrollRect || itemTransform is not RectTransform itemRectTransform)
                return true;

            var viewport = scrollRect.GetViewport();
            if (!itemTransform.IsChildOf(viewport))
                return true;

            var visibleArea = GetVisibleArea(viewport);
            var itemScrollOffset = scrollRect == ownerScrollRect ? scrollOffset : Vector2.zero;
            var scrolledVisibleMin = visibleArea.min - itemScrollOffset;
            var scrolledVisibleMax = visibleArea.max - itemScrollOffset;
            var sweptMin = Vector2.Min(visibleArea.min, scrolledVisibleMin);
            var sweptMax = Vector2.Max(visibleArea.max, scrolledVisibleMax);
            var sweptArea = Rect.MinMaxRect(sweptMin.x, sweptMin.y, sweptMax.x, sweptMax.y);
            var position = itemTransform.GetLocalPositionIn(viewport);
            var itemRect = itemRectTransform.rect;
            var itemMin = position + itemRect.position;
            var itemArea = new Rect(itemMin, itemRect.size);
            return itemArea.Overlaps(sweptArea);
        }

        // An unclipped viewport shows whatever fits on screen, but the window around it is usually still sliding or
        // scaling when items are collected, so the screen is approximated by a canvas-sized area around the viewport.
        private Rect GetVisibleArea(RectTransform viewport)
        {
            var viewportRect = viewport.rect;
            var isClipped = viewport.TryGetComponent<RectMask2D>(out _) || viewport.TryGetComponent<Mask>(out _);
            if (isClipped)
                return viewportRect;

            var canvas = viewport.GetComponentInParent<Canvas>();
            var canvasTransform = (RectTransform)canvas.rootCanvas.transform;
            var canvasRect = canvasTransform.rect;
            var areaSize = Vector2.Max(viewportRect.size, canvasRect.size);
            var halfAreaSize = areaSize * 0.5f;
            var areaMin = viewportRect.center - halfAreaSize;
            return new Rect(areaMin, areaSize);
        }

        private void AssignSteps(SequentialShowOrder order)
        {
            var step = -1;
            var rowY = float.NaN;
            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (!item.IsInViewport)
                    continue;

                var isNewStep = order == SequentialShowOrder.Items
                                || float.IsNaN(rowY)
                                || Mathf.Abs(rowY - item.Position.y) > SequentialShowItem.RowTolerance;
                if (isNewStep)
                {
                    step++;
                    rowY = item.Position.y;
                }

                _items[i] = item.WithStep(step);
            }
        }
    }
}
#endif
