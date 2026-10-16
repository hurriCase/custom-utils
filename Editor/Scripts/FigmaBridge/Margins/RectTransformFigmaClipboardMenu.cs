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

        private static readonly RectTransformFigmaClipboardMenu _menu = new();

        [MenuItem(PasteMenuPath)]
        private static void PasteFromFigma(MenuCommand menuCommand)
        {
            _menu.Paste(menuCommand);
        }

        protected override void Apply(RectTransform target, RectTransformFigmaClipboardData clipboardData)
        {
            RectTransformFigmaApplier.Apply(target, clipboardData);
        }
    }
}
