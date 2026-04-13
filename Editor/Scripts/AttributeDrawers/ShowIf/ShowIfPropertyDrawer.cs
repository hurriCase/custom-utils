using System;
using CustomUtils.Editor.Scripts.Extensions;
using CustomUtils.Runtime.Attributes.ShowIf;
using UnityEditor;

namespace CustomUtils.Editor.Scripts.AttributeDrawers.ShowIf
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    internal sealed partial class ShowIfPropertyDrawer : PropertyDrawer
    {
        private bool TryGetProperty(SerializedProperty property, out SerializedProperty serializedProperty)
        {
            var showIfAttribute = (ShowIfAttribute)attribute;
            var fieldName = showIfAttribute.ConditionalSourceField;

            serializedProperty = property.serializedObject.FindProperty(fieldName)
                                 ?? property.serializedObject.FindField(fieldName);

            if (serializedProperty != null)
                return true;

            var propertyPath = property.propertyPath;
            var lastDot = propertyPath.LastIndexOf('.');

            if (lastDot < 0)
                return false;

            var parentPath = propertyPath[..lastDot];
            serializedProperty = property.serializedObject.FindProperty($"{parentPath}.{fieldName}")
                                 ?? property.serializedObject.FindField($"{parentPath}.{fieldName}");

            return serializedProperty != null;
        }

        private bool ShouldShow(SerializedProperty sourceProperty, ShowIfAttribute showIfAttribute)
        {
            var expectedValue = showIfAttribute.ExpectedValues;
            if (expectedValue == null)
                return showIfAttribute.ShowType == ShowType.True ? sourceProperty.boolValue : !sourceProperty.boolValue;

            return Array.Exists(expectedValue, expected => sourceProperty.GetFieldValue()?.Equals(expected) ?? false);
        }
    }
}
