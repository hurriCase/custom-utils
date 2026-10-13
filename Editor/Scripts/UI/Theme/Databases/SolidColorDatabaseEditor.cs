using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.Theme.Databases;
using CustomUtils.Runtime.UI.Theme.ThemeColors;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.UI.Theme.Databases
{
    [CreateAssetMenu(
        fileName = nameof(SolidColorDatabaseEditor),
        menuName = MenuPath + nameof(SolidColorDatabaseEditor)
    )]
    internal sealed class SolidColorDatabaseEditor :
        ThemeDatabaseEditorBase<SolidColorDatabase, ThemeSolidColor, Color>
    {
        protected override ThemeSolidColor CreateThemeColor(string colorName, Color color)
            => ThemeSolidColor.Create(colorName, color);

        protected override bool IsSameColor(Color first, Color second) => first.Approximately(second);

        protected override void DrawColorField(Rect rect, string label, Color color)
        {
            EditorGUI.ColorField(rect, label, color);
        }
    }
}
