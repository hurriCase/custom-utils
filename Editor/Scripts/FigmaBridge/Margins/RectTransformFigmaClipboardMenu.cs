using CustomUtils.Editor.Scripts.FigmaBridge.Clipboard;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Margins
{
    internal sealed class RectTransformFigmaClipboardMenu :
        FigmaClipboardMenuBase<RectTransform, RectTransformFigmaClipboardData>
    {
        protected override string ExpectedSource => "FigmaRectTransformBridge";
        protected override string UndoName => "Paste Figma Margins";

        private const string PasteMenuPath = "CONTEXT/" + nameof(RectTransform) + "/Paste Margins From Figma";
        private const float MinAnchorValue = 0f;
        private const float MaxAnchorValue = 1f;
        private const float RotationEpsilonDegrees = 0.01f;

        private static readonly Vector2 _topLeftAnchor = new(MinAnchorValue, MaxAnchorValue);
        private static readonly Vector2 _centerPivot = new(0.5f, 0.5f);

        private static readonly RectTransformFigmaClipboardMenu _menu = new();

        [MenuItem(PasteMenuPath)]
        private static void PasteFromFigma(MenuCommand menuCommand)
        {
            _menu.Paste(menuCommand);
        }

        protected override void Apply(RectTransform target, RectTransformFigmaClipboardData clipboardData)
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
        private RectTransformLayout GetRotatedLayout(RectTransformFigmaClipboardData clipboardData)
        {
            var centerX = clipboardData.Left + clipboardData.Width / 2f;
            var centerY = -clipboardData.Top - clipboardData.Height / 2f;
            var center = new Vector2(centerX, centerY);
            return new RectTransformLayout(_topLeftAnchor, _centerPivot, center);
        }

        private RectTransformLayout GetNearestCornerLayout(RectTransformFigmaClipboardData clipboardData)
        {
            var corner = DetermineNearestCornerAnchor(clipboardData);
            var isRightAnchored = Mathf.Approximately(corner.x, MaxAnchorValue);
            var isTopAnchored = Mathf.Approximately(corner.y, MaxAnchorValue);
            var horizontalOffset = isRightAnchored ? -clipboardData.Right : clipboardData.Left;
            var verticalOffset = isTopAnchored ? -clipboardData.Top : clipboardData.Bottom;
            var offset = new Vector2(horizontalOffset, verticalOffset);
            return new RectTransformLayout(corner, corner, offset);
        }

        private Vector2 DetermineNearestCornerAnchor(RectTransformFigmaClipboardData clipboardData)
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
