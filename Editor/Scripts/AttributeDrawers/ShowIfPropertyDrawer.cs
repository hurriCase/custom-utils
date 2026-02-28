using CustomUtils.Editor.Scripts.Extensions;
using CustomUtils.Runtime.Attributes.ShowIf;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUtils.Editor.Scripts.AttributeDrawers
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    internal sealed class ShowIfPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var showIfAttribute = (ShowIfAttribute)attribute;
            var container = new VisualElement();
            var propertyField = new PropertyField(property);

            container.Add(propertyField);

            if (!TryGetProperty(property, out var sourceProperty))
                return container;

            UpdateVisibility(container, sourceProperty.boolValue, showIfAttribute.ShowType);

            propertyField.TrackPropertyValue(sourceProperty,
                changedProperty => UpdateVisibility(container, changedProperty.boolValue, showIfAttribute.ShowType));

            return container;
        }

        private void UpdateVisibility(VisualElement element, bool value, ShowType showType)
        {
            element.style.display = ShouldShow(value, showType) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var showIfAttribute = (ShowIfAttribute)attribute;
            if (!TryGetProperty(property, out var sourceProperty)
                && ShouldShow(sourceProperty.boolValue, showIfAttribute.ShowType))
                EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var showIfAttribute = (ShowIfAttribute)attribute;
            if (!TryGetProperty(property, out var sourceProperty)
                || !ShouldShow(sourceProperty.boolValue, showIfAttribute.ShowType))
                return -EditorGUIUtility.standardVerticalSpacing;

            return EditorGUI.GetPropertyHeight(property, label);
        }

        private bool TryGetProperty(SerializedProperty property, out SerializedProperty serializedProperty)
        {
            var showIfAttribute = (ShowIfAttribute)attribute;
            var serializedObject = property.serializedObject;
            var fieldName = showIfAttribute.ConditionalSourceField;
            serializedProperty = serializedObject.FindProperty(fieldName) ?? serializedObject.FindField(fieldName);

            return serializedProperty != null;
        }

        private bool ShouldShow(bool value, ShowType showType) => showType == ShowType.True ? value : !value;
    }
}