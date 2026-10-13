using CustomUtils.Editor.Scripts.FigmaBridge.Clipboard;
using CustomUtils.Runtime.UI.Theme;
using UnityEditor;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Halftone
{
    internal sealed class HalftoneFigmaClipboardMenu :
        FigmaClipboardMenuBase<ThemeComponent, HalftoneFigmaClipboardData>
    {
        protected override string ExpectedSource => HalftoneFigmaClipboardData.BridgeSource;
        protected override string UndoName => "Paste Figma Halftone";

        private const string PasteMenuPath = "CONTEXT/" + nameof(ThemeComponent) + "/Paste Halftone From Figma";

        private static readonly HalftoneFigmaClipboardMenu _menu = new();

        [MenuItem(PasteMenuPath)]
        private static void PasteFromFigma(MenuCommand menuCommand)
        {
            _menu.Paste(menuCommand);
        }

        protected override void Apply(ThemeComponent target, HalftoneFigmaClipboardData clipboardData)
        {
            var halftoneProperties = clipboardData.ToHalftoneProperties();
            target.SetHalftone(halftoneProperties);
        }
    }
}
