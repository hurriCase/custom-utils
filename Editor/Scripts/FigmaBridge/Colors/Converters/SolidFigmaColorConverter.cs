using CustomUtils.Editor.Scripts.FigmaBridge.Colors.Data;
using CustomUtils.Editor.Scripts.UI.Theme.Databases;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Colors.Converters
{
    [CreateAssetMenu(
        fileName = nameof(SolidFigmaColorConverter),
        menuName = MenuPath + nameof(SolidFigmaColorConverter)
    )]
    internal sealed class SolidFigmaColorConverter : FigmaColorConverterBase
    {
        [SerializeField] private SolidColorDatabaseEditor _databaseEditor;

        internal override string GetOrAddColor(ColorFigmaClipboardData clipboardData)
            => _databaseEditor.GetOrAdd(clipboardData.Color.Value, clipboardData.Name);
    }
}
