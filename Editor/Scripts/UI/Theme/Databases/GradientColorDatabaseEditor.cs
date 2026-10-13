using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.Theme.Databases;
using CustomUtils.Runtime.UI.Theme.ThemeColors;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.UI.Theme.Databases
{
    [CreateAssetMenu(
        fileName = nameof(GradientColorDatabaseEditor),
        menuName = MenuPath + nameof(GradientColorDatabaseEditor)
    )]
    internal sealed class GradientColorDatabaseEditor :
        ThemeDatabaseEditorBase<GradientColorDatabase, ThemeGradientColor, Gradient>
    {
        protected override ThemeGradientColor CreateThemeColor(string colorName, Gradient color)
            => ThemeGradientColor.Create(colorName, color);

        protected override bool IsSameColor(Gradient first, Gradient second) => first.Approximately(second);

        protected override void DrawColorField(Rect rect, string label, Gradient color)
        {
            EditorGUI.GradientField(rect, label, color);
        }
    }
}
