using CustomUtils.Editor.Scripts.CustomEditorUtilities.Scopes;
using CustomUtils.Editor.Scripts.FigmaBridge.Clipboard;
using CustomUtils.Editor.Scripts.FigmaBridge.Colors;
using CustomUtils.Editor.Scripts.FigmaBridge.Colors.Data;
using CustomUtils.Editor.Scripts.FigmaBridge.Margins;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Corners;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Effects;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers;
using CustomUtils.Runtime.UI.Theme;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.FigmaBridge.ProceduralImages
{
    internal sealed class ProceduralImageFigmaClipboardMenu :
        FigmaClipboardMenuBase<ProceduralImage, ProceduralImageFigmaClipboardData>
    {
        protected override string ExpectedSource => "FigmaProceduralImageBridge";
        protected override string UndoName => "Paste Figma Procedural Image";

        private const string PasteMenuPath = "CONTEXT/" + nameof(ProceduralImage) + "/Paste From Figma";

        private static readonly ProceduralImageFigmaClipboardMenu _menu = new();

        [MenuItem(PasteMenuPath)]
        private static void PasteFromFigma(MenuCommand menuCommand)
        {
            _menu.Paste(menuCommand);
        }

        protected override void Apply(ProceduralImage target, ProceduralImageFigmaClipboardData clipboardData)
        {
            ApplyRectTransform(target.rectTransform, clipboardData.RectTransform);
            ApplyCornerOffsets(target, clipboardData.CornerOffsets);
            ApplyCornerRadius(target, clipboardData.CornerRadius);
            ApplyFill(target, clipboardData.Fill);
            ApplyOutline(target, clipboardData.Outline);
            ApplyShadow(target, clipboardData.Shadow);
        }

        private void ApplyRectTransform(RectTransform target, RectTransformFigmaClipboardData rectTransformData)
        {
            if (rectTransformData == null)
                return;

            using (new RecordUndoScope(target, UndoName))
                RectTransformFigmaApplier.Apply(target, rectTransformData);
        }

        private void ApplyCornerOffsets(ProceduralImage target, CornerOffsetsFigmaClipboardData cornerOffsetsData)
        {
            if (cornerOffsetsData == null)
                return;

            var offsets = new CornerOffsets(
                cornerOffsetsData.TopLeft.Value,
                cornerOffsetsData.TopRight.Value,
                cornerOffsetsData.BottomRight.Value,
                cornerOffsetsData.BottomLeft.Value,
                CornerOffsetMode.Absolute);

            target.SetCornerOffsets(offsets);
        }

        private void ApplyCornerRadius(Component target, float? cornerRadius)
        {
            if (cornerRadius is not { } radius)
                return;

            if (!target.TryGetComponent<UniformCornerModifier>(out var modifier))
            {
                Debug.LogWarning($"[{nameof(ProceduralImageFigmaClipboardMenu)}::{nameof(ApplyCornerRadius)}] " +
                                 $"'{target.name}' has no {nameof(UniformCornerModifier)}, radius {radius} skipped");
                return;
            }

            using (new RecordUndoScope(modifier, UndoName))
                modifier.SetRadius(radius);
        }

        private void ApplyFill(Component target, ColorFigmaClipboardData fillData)
        {
            if (fillData == null)
                return;

            if (!target.TryGetComponent<ThemeComponent>(out var themeComponent))
            {
                Debug.LogWarning($"[{nameof(ProceduralImageFigmaClipboardMenu)}::{nameof(ApplyFill)}] " +
                                 $"'{target.name}' has no {nameof(ThemeComponent)}, fill skipped");
                return;
            }

            var style = ThemeStyleFigmaFactory.Create(fillData, themeComponent.Style);

            using (new RecordUndoScope(themeComponent, UndoName))
                themeComponent.UpdateStyle(style);
        }

        // No stroke in Figma removes the outline but keeps its style for later.
        private void ApplyOutline(ProceduralImage target, FigmaOutlineData outlineData)
        {
            var currentOutline = target.Outline;
            if (outlineData == null)
            {
                var hiddenOutline = new ProceduralOutline(0f, currentOutline.Alignment, currentOutline.Style);
                target.SetOutline(hiddenOutline);
                return;
            }

            var style = ThemeStyleFigmaFactory.Create(outlineData.Style, currentOutline.Style);
            var outline = new ProceduralOutline(outlineData.Width, outlineData.Alignment, style);
            target.SetOutline(outline);
        }

        private void ApplyShadow(ProceduralImage target, FigmaShadowData shadowData)
        {
            var currentShadow = target.Shadow;
            var shadow = shadowData == null
                ? new ProceduralShadow(false, currentShadow.Offset, currentShadow.Color)
                : new ProceduralShadow(true, shadowData.Offset.Value, shadowData.Color.Value);

            target.SetShadow(shadow);
        }
    }
}
