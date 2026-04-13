using CustomUtils.Runtime.Attributes.ShowIf;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CustomUtils.Editor.Scripts.AttributeDrawers.ShowIf
{
    internal sealed partial class ShowIfPropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var showIfAttribute = (ShowIfAttribute)attribute;
            var container = new VisualElement();
            var propertyField = new PropertyField(property);

            container.Add(propertyField);

            if (!TryGetProperty(property, out var sourceProperty))
                return container;

            UpdateVisibility(container, sourceProperty, showIfAttribute);

            propertyField.TrackPropertyValue(sourceProperty,
                changedProperty => UpdateVisibility(container, changedProperty, showIfAttribute));

            return container;
        }

        private void UpdateVisibility(
            VisualElement element,
            SerializedProperty serializedProperty,
            ShowIfAttribute showIfAttribute)
        {
            element.style.display = ShouldShow(serializedProperty, showIfAttribute)
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }
}