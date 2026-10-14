using System.Collections.Generic;
using CustomUtils.Editor.Scripts.FigmaBridge.Colors.Data;
using CustomUtils.Editor.Scripts.UI.Theme.Databases;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Colors.Converters
{
    [CreateAssetMenu(
        fileName = nameof(GradientFigmaColorConverter),
        menuName = MenuPath + nameof(GradientFigmaColorConverter)
    )]
    internal sealed class GradientFigmaColorConverter : FigmaColorConverterBase
    {
        [SerializeField] private GradientColorDatabaseEditor _databaseEditor;

        internal override string GetOrAddColor(ColorFigmaClipboardData clipboardData)
        {
            var gradient = CreateGradient(clipboardData.Stops);
            return _databaseEditor.GetOrAdd(gradient, clipboardData.Name);
        }

        private Gradient CreateGradient(IReadOnlyList<FigmaGradientStopData> stops)
        {
            var colorKeys = new GradientColorKey[stops.Count];
            var alphaKeys = new GradientAlphaKey[stops.Count];

            for (var i = 0; i < stops.Count; i++)
            {
                var stop = stops[i];
                var color = stop.Value;
                colorKeys[i] = new GradientColorKey(color, stop.Position);
                alphaKeys[i] = new GradientAlphaKey(color.a, stop.Position);
            }

            var gradient = new Gradient();
            gradient.SetKeys(colorKeys, alphaKeys);
            return gradient;
        }
    }
}
