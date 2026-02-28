using System.Collections.Generic;
using CustomUtils.Runtime.Attributes;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.AttributeDrawers
{
    [CustomPropertyDrawer(typeof(DistinctEnumAttribute))]
    internal sealed class DistinctEnumDrawer : PropertyDrawer
    {
        private readonly HashSet<int> _usedValues = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.isArray)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            if (property.propertyType != SerializedPropertyType.Enum)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var currentValue = property.enumValueIndex;
            CollectUsedEnumValues(property);

            var displayNames = new List<string>();
            var enumIndices = new List<int>();

            for (var i = 0; i < property.enumNames.Length; i++)
            {
                if (i != currentValue && _usedValues.Contains(i))
                    continue;

                displayNames.Add(property.enumNames[i]);
                enumIndices.Add(i);
            }

            var currentIndex = enumIndices.IndexOf(currentValue);

            EditorGUI.BeginChangeCheck();
            var selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, displayNames.ToArray());

            if (EditorGUI.EndChangeCheck() && selectedIndex >= 0)
                property.enumValueIndex = enumIndices[selectedIndex];
        }

        private void CollectUsedEnumValues(SerializedProperty property)
        {
            _usedValues.Clear();
            var parentProperty = FindParentProperty(property);

            if (parentProperty is { isArray: true })
                GetUsedEnumValuesFromArray(parentProperty);
            else
                GetUsedEnumValuesWithinComponent(property);
        }

        private void GetUsedEnumValuesFromArray(SerializedProperty property)
        {
            for (var i = 0; i < property.arraySize; i++)
            {
                var element = property.GetArrayElementAtIndex(i);

                if (element.propertyPath == property.propertyPath)
                    continue;

                if (element.propertyType == SerializedPropertyType.Enum)
                    _usedValues.Add(element.enumValueIndex);
            }
        }

        private void GetUsedEnumValuesWithinComponent(SerializedProperty property)
        {
            var prop = property.serializedObject.GetIterator();
            while (prop.NextVisible(true))
            {
                if (prop.propertyPath != property.propertyPath &&
                    prop.propertyType == SerializedPropertyType.Enum)
                    _usedValues.Add(prop.enumValueIndex);
            }
        }

        private SerializedProperty FindParentProperty(SerializedProperty property)
        {
            if (!property.propertyPath.Contains("."))
                return null;

            var path = property.propertyPath;
            var lastDot = path.LastIndexOf('.');

            while (lastDot > 0)
            {
                path = path[..lastDot];
                var parent = property.serializedObject.FindProperty(path);

                if (parent != null)
                    return parent;

                lastDot = path.LastIndexOf('.');
            }

            return null;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property, label, true);
    }
}