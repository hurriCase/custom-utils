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

        internal void Collect(SequentialShowAnimation owner, SequentialShowOrder order, bool isBottomToTop)
        {
            _items.Clear();

            owner.GetComponentsInChildren(_foundItems);
            foreach (var foundItem in _foundItems)
                TryAddItem(owner, foundItem.transform, foundItem, null);

            owner.GetComponentsInChildren(_foundNestedAnimations);
            foreach (var nestedAnimation in _foundNestedAnimations)
                TryAddItem(owner, nestedAnimation.transform, null, nestedAnimation);

            _items.Sort();

            if (isBottomToTop)
                _items.Reverse();

            AssignSteps(order);
        }

        // ScrollRect.normalizedPosition goes through world matrices, which collapse while an ancestor is scaled to
        // zero (a window expanding from scale 0), so the content is aligned to the viewport's top-left locally.
        internal void ResetScroll(ScrollRect scrollRect)
        {
            if (!scrollRect)
                return;

            scrollRect.StopMovement();

            var viewport = scrollRect.viewport ? scrollRect.viewport : (RectTransform)scrollRect.transform;
            var content = scrollRect.content;
            var viewportRect = viewport.rect;
            var contentRect = content.rect;
            var contentScale = content.localScale;
            var viewportTopLeft = new Vector2(viewportRect.xMin, viewportRect.yMax);
            var contentTopLeftOffset =
                new Vector2(contentRect.xMin * contentScale.x, contentRect.yMax * contentScale.y);
            var contentTopLeft = (Vector2)content.localPosition + contentTopLeftOffset;
            var offset = viewportTopLeft - contentTopLeft;
            content.anchoredPosition += offset;
        }

        private void TryAddItem(
            SequentialShowAnimation owner,
            Transform itemTransform,
            TweenSequentialShowItem item,
            SequentialShowAnimation nestedAnimation)
        {
            if (!IsOwnItem(owner, itemTransform))
                return;

            var position = GetLocalPosition(itemTransform, owner.transform);
            var isInViewport = IsInViewport(itemTransform);
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

        private bool IsInViewport(Transform itemTransform)
        {
            var scrollRect = itemTransform.GetComponentInParent<ScrollRect>();
            if (!scrollRect || itemTransform is not RectTransform itemRectTransform)
                return true;

            var viewport = scrollRect.viewport ? scrollRect.viewport : (RectTransform)scrollRect.transform;
            if (!itemTransform.IsChildOf(viewport))
                return true;

            var position = GetLocalPosition(itemTransform, viewport);
            var itemRect = itemRectTransform.rect;
            var itemMin = position + itemRect.position;
            var itemArea = new Rect(itemMin, itemRect.size);
            return itemArea.Overlaps(viewport.rect);
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

        // Items are collected when the show starts, often while a window ancestor is still scaled to zero; world
        // matrices collapse then and rows merge, so the position is accumulated through local transforms instead.
        private Vector2 GetLocalPosition(Transform target, Transform space)
        {
            var position = target.localPosition;
            var parent = target.parent;
            while (parent != space)
            {
                var scaledPosition = Vector3.Scale(parent.localScale, position);
                position = parent.localPosition + parent.localRotation * scaledPosition;
                parent = parent.parent;
            }

            return position;
        }
    }
}
#endif
