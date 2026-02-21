using System.Collections.Generic;
using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using CustomUtils.Editor.Scripts.Extensions;
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
        private EditorStateControls _editorStateControls;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            _editorStateControls ??=
                new EditorStateControls(property.serializedObject.targetObject, property.serializedObject);

            var colorType = property.GetPropertyFromParent<ColorType>(nameof(ColorData.ColorType));

            return colorType != ColorType.None && string.IsNullOrEmpty(property.stringValue) is false
                ? EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing
                : EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var colorType = property.GetPropertyFromParent<ColorType>(nameof(ColorData.ColorType));

            if (colorType == ColorType.None)
                return;

            if (TryGetColorNamesForType(colorType, out var colorNames) is false)
            {
                var message = StringFormatter.Format("No {0} colors found in database.", colorType);
                EditorVisualControls.WarningBox(position, message);
                return;
            }

            var colorName = _editorStateControls.Dropdown(property, colorNames, position);

            DrawColorPreview(colorName, colorType, position);
        }

        private void DrawColorPreview(string colorName, ColorType colorType, Rect position)
        {
            var previewRect = new Rect(
                position.x,
                position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                position.width,
                EditorGUIUtility.singleLineHeight
            );

            switch (colorType)
            {
                case ColorType.Solid:
                    if (SolidColorDatabase.Instance.TryGetColorByName(colorName, out var previewColor))
                        EditorVisualControls.ColorField(previewRect, "Preview", previewColor);
                    break;

                case ColorType.GraphicGradient or ColorType.TextGradient:
                    if (GradientColorDatabase.Instance.TryGetColorByName(colorName, out var gradient))
                        EditorVisualControls.GradientField(previewRect, "Preview", gradient);
                    break;
            }
        }

        private bool TryGetColorNamesForType(ColorType colorType, out List<string> colorNames)
        {
            colorNames = colorType switch
            {
                ColorType.Solid => SolidColorDatabase.Instance.GetColorNames(),
                ColorType.GraphicGradient or ColorType.TextGradient => GradientColorDatabase.Instance.GetColorNames(),
                _ => null
            };

            return colorNames != null && colorNames.Count != 0;
        }
    }
}