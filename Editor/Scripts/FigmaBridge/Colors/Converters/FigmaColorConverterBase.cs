using CustomUtils.Editor.Scripts.FigmaBridge.Colors.Data;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Colors.Converters
{
    internal abstract class FigmaColorConverterBase : ScriptableObject
    {
        internal const string MenuPath = "CustomUtils/Figma Bridge/";

        internal abstract string GetOrAddColor(ColorFigmaClipboardData clipboardData);
    }
}
