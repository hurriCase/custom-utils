using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using CustomUtils.Editor.Scripts.Extensions;
using CustomUtils.Editor.Scripts.UI.Theme.Databases;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Formatter;
using CustomUtils.Runtime.UI.Theme;
using CustomUtils.Runtime.UI.Theme.Databases;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.UI.Theme
{
    [CustomPropertyDrawer(typeof(ThemeColorNameAttribute))]
    internal sealed class ThemeColorNamePropertyDrawer : PropertyDrawer
    {
        private const int DefaultRowCount = 1;
        private const int ExpandedRowCount = 2;

        private EditorStateControls _editorStateControls;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            _editorStateControls ??=
                new EditorStateControls(property.serializedObject.targetObject, property.serializedObject);

            var colorType = property.GetPropertyFromParent<ColorType>(nameof(ColorData.ColorType));

            var rowCount = colorType != ColorType.None && !string.IsNullOrEmpty(property.stringValue)
                ? ExpandedRowCount
                : DefaultRowCount;

            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * rowCount;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var colorType = property.GetPropertyFromParent<ColorType>(nameof(ColorData.ColorType));

            if (colorType == ColorType.None)
                return;

            var database = ThemeDatabaseConfig.Instance.GetDatabase(colorType);
            var colorNames = database.GetColorNames();
            var colorGuids = database.GetColorGuids();

            if (colorNames.IsNullOrEmpty() || colorGuids.IsNullOrEmpty())
            {
                var message = StringFormatter.Format("No {0} colors found in database.", colorType);
                EditorVisualControls.WarningBox(position, message);
                return;
            }

            var colorTypeRect = GetRowRect(position, 0);
            var (_, colorGuid) = _editorStateControls.Dropdown(property, colorNames, colorGuids, colorTypeRect);

            var previewRect = GetRowRect(position, 1);
            var databaseEditor = ThemeDatabaseEditorConfig.Instance.Editors[colorType];
            databaseEditor.DrawPreview(previewRect, colorGuid);
        }

        private static Rect GetRowRect(Rect position, int rowIndex) => new(
            position.x,
            position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * rowIndex,
            position.width,
            EditorGUIUtility.singleLineHeight
        );
    }
}