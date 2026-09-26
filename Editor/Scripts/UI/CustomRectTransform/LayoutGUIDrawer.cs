#if IS_RECTTRANSFORM_EXTENDED_ENABLED
using System.Reflection;
using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.UI.CustomRectTransform
{
    internal sealed class LayoutGUIDrawer
    {
        private static readonly MethodInfo _multiFieldPrefixLabelMethod;
        private static readonly MethodInfo _calcPrefixLabelWidthMethod;

        private readonly EditorStateControls _stateControls;

        static LayoutGUIDrawer()
        {
            var editorGUIType = typeof(EditorGUI);

            _multiFieldPrefixLabelMethod = editorGUIType.GetMethod("MultiFieldPrefixLabel",
                BindingFlags.Static | BindingFlags.NonPublic,
                null, new[] { typeof(Rect), typeof(int), typeof(GUIContent), typeof(int) }, null);

            _calcPrefixLabelWidthMethod = editorGUIType.GetMethod("CalcPrefixLabelWidth",
                BindingFlags.Static | BindingFlags.NonPublic,
                null, new[] { typeof(GUIContent), typeof(GUIStyle) }, null);
        }

        internal LayoutGUIDrawer(EditorStateControls stateControls)
        {
            _stateControls = stateControls;
        }

        internal void DrawVector2FieldStacked(in Vector2FieldData field, ref float xValue, ref float yValue)
        {
            var (firstColumn, secondColumn) = DrawStackedLabel(field.Label);

            DrawStackedField(firstColumn, field.XLabel, ref xValue, field.IsXMixed);
            DrawStackedField(secondColumn, field.YLabel, ref yValue, field.IsYMixed);
        }

        internal void DrawVector2FieldHorizontal(in Vector2FieldData field, ref float xValue, ref float yValue)
        {
            var controlRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            var controlId = GUIUtility.GetControlID(field.Label.GetHashCode(), FocusType.Keyboard, controlRect);

            var fieldRect = (Rect)_multiFieldPrefixLabelMethod.Invoke(null,
                new object[] { controlRect, controlId, new GUIContent(field.Label), 2 });

            var originalLabelWidth = EditorGUIUtility.labelWidth;
            var originalIndentLevel = EditorGUI.indentLevel;

            var subLabelWidth = (float)_calcPrefixLabelWidthMethod.Invoke(null,
                new object[] { new GUIContent(field.XLabel), EditorStyles.label });

            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = subLabelWidth;

            var (xRect, yRect) = GetHorizontalFieldRects(fieldRect);

            EditorGUI.showMixedValue = field.IsXMixed;
            xValue = _stateControls.FloatField(xRect, $"{field.Label} {field.XLabel}",
                new GUIContent(field.XLabel), xValue);
            EditorGUI.showMixedValue = field.IsYMixed;
            yValue = _stateControls.FloatField(yRect, $"{field.Label} {field.YLabel}",
                new GUIContent(field.YLabel), yValue);
            EditorGUI.showMixedValue = false;

            EditorGUIUtility.labelWidth = originalLabelWidth;
            EditorGUI.indentLevel = originalIndentLevel;
        }

        internal void DrawVector2ReadOnly(in Vector2FieldData field, float xValue, float yValue)
        {
            var (firstColumn, secondColumn) = DrawStackedLabel(field.Label);

            DrawStackedReadOnlyField(firstColumn, field.XLabel, xValue, field.IsXMixed);
            DrawStackedReadOnlyField(secondColumn, field.YLabel, yValue, field.IsYMixed);
        }

        private (Rect first, Rect second) DrawStackedLabel(string label)
        {
            var controlRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight * 2f);
            controlRect.height = EditorGUIUtility.singleLineHeight;

            var labelRect = new Rect(controlRect.x, controlRect.y, EditorGUIUtility.labelWidth, controlRect.height);
            EditorVisualControls.LabelField(labelRect, label);

            return GetTwoColumnRects(controlRect);
        }

        private void DrawStackedReadOnlyField(Rect columnRect, string labelText, float value, bool isMixed)
        {
            var fieldRect = DrawStackedFieldLabel(columnRect, labelText);

            EditorGUI.showMixedValue = isMixed;
            EditorVisualControls.ReadOnlyFloatField(fieldRect, value);
            EditorGUI.showMixedValue = false;
        }

        private void DrawStackedField(Rect columnRect, string labelText, ref float value, bool isMixed)
        {
            var fieldRect = DrawStackedFieldLabel(columnRect, labelText);

            EditorGUI.showMixedValue = isMixed;
            value = _stateControls.FloatField(fieldRect, labelText, value);
            EditorGUI.showMixedValue = false;
        }

        private Rect DrawStackedFieldLabel(Rect columnRect, string labelText)
        {
            var labelRect = new Rect(columnRect.x, columnRect.y, columnRect.width, EditorGUIUtility.singleLineHeight);
            EditorVisualControls.LabelField(labelRect, labelText);

            return new Rect(columnRect.x, columnRect.y + EditorGUIUtility.singleLineHeight, columnRect.width,
                EditorGUIUtility.singleLineHeight);
        }

        private (Rect first, Rect second) GetTwoColumnRects(Rect totalRect)
        {
            totalRect.xMin += EditorGUIUtility.labelWidth - 1f;

            var columnWidth = (totalRect.width - 2f) / 2f;

            var firstColumn = new Rect(totalRect.x, totalRect.y, columnWidth, EditorGUIUtility.singleLineHeight * 2f);
            var secondColumn = new Rect(totalRect.x + columnWidth + 2f, totalRect.y, columnWidth,
                EditorGUIUtility.singleLineHeight * 2f);

            return (firstColumn, secondColumn);
        }

        private (Rect xRect, Rect yRect) GetHorizontalFieldRects(Rect fieldRect)
        {
            var columnWidth = (fieldRect.width - 2f) / 2f;

            var xRect = new Rect(fieldRect.x, fieldRect.y, columnWidth, fieldRect.height);
            var yRect = new Rect(fieldRect.x + columnWidth + 2f, fieldRect.y, columnWidth, fieldRect.height);

            return (xRect, yRect);
        }
    }
}
#endif
