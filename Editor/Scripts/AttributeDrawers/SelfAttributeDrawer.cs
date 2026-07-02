using CustomUtils.Editor.Scripts.Extensions;
using CustomUtils.Runtime.Attributes;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.AttributeDrawers
{
    [CustomPropertyDrawer(typeof(SelfAttribute))]
    internal class SelfAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            AutoAssignComponent(property);

            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }

        private void AutoAssignComponent(SerializedProperty property)
        {
            if (!property.TryGetComponent(fieldInfo.FieldType, out var targetComponent) || targetComponent == property.objectReferenceValue)
                return;

            property.objectReferenceValue = targetComponent;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}