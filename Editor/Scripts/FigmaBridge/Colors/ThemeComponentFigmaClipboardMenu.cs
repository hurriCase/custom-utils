using CustomUtils.Editor.Scripts.CustomEditorUtilities.Scopes;
using CustomUtils.Editor.Scripts.FigmaBridge.Clipboard;
using CustomUtils.Editor.Scripts.FigmaBridge.Colors.Converters;
using CustomUtils.Editor.Scripts.FigmaBridge.Colors.Data;
using CustomUtils.Runtime.UI.Theme;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Colors
{
    internal sealed class ThemeComponentFigmaClipboardMenu :
        FigmaClipboardMenuBase<ThemeComponent, ColorFigmaClipboardData>
    {
        protected override string ExpectedSource => "FigmaColorBridge";
        protected override string UndoName => "Paste Figma Color";

        private const string PasteMenuPath = "CONTEXT/" + nameof(ThemeComponent) + "/Paste Color From Figma";
        private const string GradientPasteMenuPath =
            "CONTEXT/" + nameof(GradientColorModifier) + "/Paste Color From Figma";

        private static readonly ThemeComponentFigmaClipboardMenu _menu = new();

        [MenuItem(PasteMenuPath)]
        private static void PasteFromFigma(MenuCommand menuCommand)
        {
            _menu.Paste(menuCommand);
        }

        [MenuItem(GradientPasteMenuPath)]
        private static void PasteFromFigmaIntoGradient(MenuCommand menuCommand)
        {
            _menu.Paste(menuCommand);
        }

        protected override ThemeComponent GetTarget(Object context)
            => ((Component)context).GetComponent<ThemeComponent>();

        protected override void Apply(ThemeComponent target, ColorFigmaClipboardData clipboardData)
        {
            var converter = FigmaColorConverterConfig.Instance.Converters[clipboardData.ColorType];
            var colorGuid = converter.GetOrAddColor(clipboardData);
            var colorData = new ColorData(clipboardData.ColorType, colorGuid);
            var previousModifier = target.CurrentColorModifier;

            target.UpdateColorData(colorData);

            var modifier = target.CurrentColorModifier;
            if (modifier != previousModifier)
                Undo.RegisterCreatedObjectUndo(modifier, UndoName);

            using (new RecordUndoScope(modifier, UndoName))
                converter.ApplyTo(modifier, clipboardData);

            if (clipboardData.Halftone == null)
            {
                target.DisableHalftone();
                return;
            }

            var halftoneProperties = clipboardData.Halftone.ToHalftoneProperties();
            target.SetHalftone(halftoneProperties);
        }
    }
}
