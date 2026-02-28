using System.Collections.Generic;
using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using CustomUtils.Editor.Scripts.Extensions;
using CustomUtils.Runtime.UI.ImagePixelPerUnit;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.AttributeDrawers
{
    [CustomPropertyDrawer(typeof(PixelPerUnitPopupAttribute))]
    public class PixelPerUnitDataDrawer : PropertyDrawer
    {
        private PixelPerUnitDatabase _pixelPerUnitDatabase;

        private SerializedProperty _nameProperty;
        private SerializedProperty _dimensionTypeProperty;
        private SerializedProperty _imageSizeProperty;
        private SerializedProperty _cornerRadiusProperty;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            _pixelPerUnitDatabase = PixelPerUnitDatabase.Instance;

            _nameProperty = property.FindFieldRelative(nameof(PixelPerUnitData.Name));
            _dimensionTypeProperty = property.FindFieldRelative(nameof(PixelPerUnitData.DimensionType));
            _imageSizeProperty = property.FindFieldRelative(nameof(PixelPerUnitData.ImageSize));
            _cornerRadiusProperty = property.FindFieldRelative(nameof(PixelPerUnitData.CornerRadius));

            if (!ValidatePixelPerUnitTypes(out _))
                return EditorGUIUtility.singleLineHeight * 1.5f + EditorGUIUtility.standardVerticalSpacing;

            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!ValidatePixelPerUnitTypes(out var pixelPerUnitTypeNames))
            {
                EditorVisualControls.WarningBox(position, "No pixel per unit types in database");
                return;
            }

            pixelPerUnitTypeNames.Insert(0, PixelPerUnitData.NoneOption);
            var entryNameProperty = property.FindFieldRelative(nameof(PixelPerUnitData.Name));
            var currentIndex = pixelPerUnitTypeNames.IndexOf(entryNameProperty.stringValue);

            if (currentIndex == -1)
            {
                currentIndex = 0;
                CopyPixelPerUnitDataToProperty(PixelPerUnitData.None, property);
            }

            var newIndex = EditorGUI.Popup(position, label.text, currentIndex, pixelPerUnitTypeNames.ToArray());

            if (newIndex == currentIndex || newIndex < 0)
                return;

            var selectedData = _pixelPerUnitDatabase.GetPixelPerUnitData(pixelPerUnitTypeNames[newIndex]);
            CopyPixelPerUnitDataToProperty(selectedData, property);
        }

        private void CopyPixelPerUnitDataToProperty(PixelPerUnitData sourceData, SerializedProperty targetProperty)
        {
            _nameProperty.stringValue = sourceData.Name;
            _dimensionTypeProperty.enumValueIndex = (int)sourceData.DimensionType;
            _imageSizeProperty.floatValue = sourceData.ImageSize;
            _cornerRadiusProperty.floatValue = sourceData.CornerRadius;

            targetProperty.serializedObject.ApplyModifiedProperties();
        }

        private bool ValidatePixelPerUnitTypes(out List<string> pixelPerUnitTypeNames)
        {
            pixelPerUnitTypeNames = _pixelPerUnitDatabase.GetPixelPerUnitTypeNames();
            return pixelPerUnitTypeNames != null && pixelPerUnitTypeNames.Count != 0;
        }
    }
}