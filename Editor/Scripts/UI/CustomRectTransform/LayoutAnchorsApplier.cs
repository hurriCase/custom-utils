using UnityEngine;

namespace CustomUtils.Editor.Scripts.UI.CustomRectTransform
{
    internal static class LayoutAnchorsApplier
    {
        internal static void Apply(RectTransform rectTransform, LayoutData layoutData)
        {
            if (layoutData.ParentWidth <= 0 || layoutData.ParentHeight <= 0)
            {
                Debug.LogWarning($"[{nameof(LayoutAnchorsApplier)}::{nameof(Apply)}] " +
                                 $"Cannot apply anchors to '{rectTransform.name}': Parent size is invalid.",
                    rectTransform);
                return;
            }

            var widthRatio = 1f / layoutData.ParentWidth;
            var heightRatio = 1f / layoutData.ParentHeight;

            rectTransform.anchorMin = new Vector2(
                widthRatio * layoutData.LeftMargin,
                heightRatio * layoutData.BottomMargin);

            rectTransform.anchorMax = new Vector2(
                1 - widthRatio * layoutData.RightMargin,
                1 - heightRatio * layoutData.TopMargin);

            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
        }
    }
}
