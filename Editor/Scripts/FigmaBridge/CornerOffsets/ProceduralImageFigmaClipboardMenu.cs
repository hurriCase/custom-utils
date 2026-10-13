using CustomUtils.Editor.Scripts.FigmaBridge.Clipboard;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage;
using UnityEditor;

namespace CustomUtils.Editor.Scripts.FigmaBridge.CornerOffsets
{
    internal sealed class ProceduralImageFigmaClipboardMenu :
        FigmaClipboardMenuBase<ProceduralImage, CornerOffsetsFigmaClipboardData>
    {
        protected override string ExpectedSource => "FigmaCornerOffsetsBridge";
        protected override string UndoName => "Paste Figma Corner Offsets";

        private const string PasteMenuPath =
            "CONTEXT/" + nameof(ProceduralImage) + "/Paste Corner Offsets From Figma";

        private static readonly ProceduralImageFigmaClipboardMenu _menu = new();

        [MenuItem(PasteMenuPath)]
        private static void PasteFromFigma(MenuCommand menuCommand)
        {
            _menu.Paste(menuCommand);
        }

        protected override void Apply(ProceduralImage target, CornerOffsetsFigmaClipboardData clipboardData)
        {
            target.CornerOffsetTopLeft = target.PixelsToStoredOffset(clipboardData.TopLeft.Value);
            target.CornerOffsetTopRight = target.PixelsToStoredOffset(clipboardData.TopRight.Value);
            target.CornerOffsetBottomRight = target.PixelsToStoredOffset(clipboardData.BottomRight.Value);
            target.CornerOffsetBottomLeft = target.PixelsToStoredOffset(clipboardData.BottomLeft.Value);
            target.SetVerticesDirty();
        }
    }
}
