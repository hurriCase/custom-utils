#if IS_RECTTRANSFORM_EXTENDED_ENABLED
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Editor.Scripts.UI.CustomRectTransform
{
    internal sealed class LayoutCalculator
    {
        internal LayoutData Calculate(Object target)
        {
            var rectTransform = target as RectTransform;
            if (!rectTransform)
                return default;

            var parentSize = CalculateParentSize(rectTransform);

            var layoutData = new LayoutData
            {
                ParentWidth = parentSize.x,
                ParentHeight = parentSize.y,
            };

            CalculateMargins(rectTransform, parentSize, ref layoutData);

            return layoutData;
        }

        private Vector2 CalculateParentSize(Transform rectTransform)
        {
            var parentRectTransform = rectTransform.parent as RectTransform;
            if (!parentRectTransform)
                return GetCanvasSize(rectTransform);

            var parentRect = parentRectTransform.rect;
            return new Vector2(Mathf.Abs(parentRect.width), Mathf.Abs(parentRect.height));
        }

        private Vector2 GetCanvasSize(Component rectTransform)
        {
            var canvas = rectTransform.GetComponentInParent<Canvas>();
            if (!canvas) return Vector2.zero;

            var canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect)
                return new Vector2(Mathf.Abs(canvasRect.rect.width), Mathf.Abs(canvasRect.rect.height));

            var canvasScaler = canvas.GetComponent<CanvasScaler>();
            if (canvasScaler && canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
                return canvasScaler.referenceResolution;

            return Vector2.zero;
        }

        private void CalculateMargins(RectTransform rectTransform, Vector2 parentSize, ref LayoutData layoutData)
        {
            if (parentSize.x <= 0 || parentSize.y <= 0)
                return;

            var anchorMin = rectTransform.anchorMin;
            var anchorMax = rectTransform.anchorMax;
            var offsetMin = rectTransform.offsetMin;
            var offsetMax = rectTransform.offsetMax;

            layoutData.LeftMargin = anchorMin.x * parentSize.x + offsetMin.x;
            layoutData.RightMargin = (1 - anchorMax.x) * parentSize.x - offsetMax.x;
            layoutData.BottomMargin = anchorMin.y * parentSize.y + offsetMin.y;
            layoutData.TopMargin = (1 - anchorMax.y) * parentSize.y - offsetMax.y;
        }
    }
}
#endif