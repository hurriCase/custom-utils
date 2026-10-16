using UnityEngine;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Margins
{
    internal static class RectTransformFigmaApplier
    {
        private const float MinAnchorValue = 0f;
        private const float MaxAnchorValue = 1f;
        private const float RotationEpsilonDegrees = 0.01f;

        private static readonly Vector2 _topLeftAnchor = new(MinAnchorValue, MaxAnchorValue);
        private static readonly Vector2 _centerPivot = new(0.5f, 0.5f);

        internal static void Apply(RectTransform target, RectTransformFigmaClipboardData clipboardData)
        {
            var rotationMagnitude = Mathf.Abs(clipboardData.Rotation);
            var isRotated = rotationMagnitude > RotationEpsilonDegrees;
            var rotation = isRotated ? clipboardData.Rotation : 0f;
            var layout = isRotated ? GetRotatedLayout(clipboardData) : GetNearestCornerLayout(clipboardData);

            target.anchorMin = layout.Anchor;
            target.anchorMax = layout.Anchor;
            target.pivot = layout.Pivot;
            target.sizeDelta = new Vector2(clipboardData.Width, clipboardData.Height);
            target.anchoredPosition = layout.Position;
            target.localEulerAngles = new Vector3(0f, 0f, rotation);
        }

        // Figma rotates a shape around its center, so a rotated shape gets a center pivot.
        // The plugin sends the unrotated top-left corner in Left and Top.
        private static RectTransformLayout GetRotatedLayout(RectTransformFigmaClipboardData clipboardData)
        {
            var centerX = clipboardData.Left + clipboardData.Width / 2f;
            var centerY = -clipboardData.Top - clipboardData.Height / 2f;
            var center = new Vector2(centerX, centerY);
            return new RectTransformLayout(_topLeftAnchor, _centerPivot, center);
        }

        private static RectTransformLayout GetNearestCornerLayout(RectTransformFigmaClipboardData clipboardData)
        {
            var corner = DetermineNearestCornerAnchor(clipboardData);
            var isRightAnchored = Mathf.Approximately(corner.x, MaxAnchorValue);
            var isTopAnchored = Mathf.Approximately(corner.y, MaxAnchorValue);
            var horizontalOffset = isRightAnchored ? -clipboardData.Right : clipboardData.Left;
            var verticalOffset = isTopAnchored ? -clipboardData.Top : clipboardData.Bottom;
            var offset = new Vector2(horizontalOffset, verticalOffset);
            return new RectTransformLayout(corner, corner, offset);
        }

        private static Vector2 DetermineNearestCornerAnchor(RectTransformFigmaClipboardData clipboardData)
        {
            var leftDistance = Mathf.Abs(clipboardData.Left);
            var rightDistance = Mathf.Abs(clipboardData.Right);
            var topDistance = Mathf.Abs(clipboardData.Top);
            var bottomDistance = Mathf.Abs(clipboardData.Bottom);
            var anchorX = leftDistance <= rightDistance ? MinAnchorValue : MaxAnchorValue;
            var anchorY = topDistance <= bottomDistance ? MaxAnchorValue : MinAnchorValue;
            return new Vector2(anchorX, anchorY);
        }
    }
}
