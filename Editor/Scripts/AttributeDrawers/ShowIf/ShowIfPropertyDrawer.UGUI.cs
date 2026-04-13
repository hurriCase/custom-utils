using CustomUtils.Runtime.Attributes.ShowIf;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.AttributeDrawers.ShowIf
{
    internal sealed partial class ShowIfPropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var showIfAttribute = (ShowIfAttribute)attribute;
            if (!TryGetProperty(property, out var sourceProperty) || ShouldShow(sourceProperty, showIfAttribute))
                EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var showIfAttribute = (ShowIfAttribute)attribute;
            if (!TryGetProperty(property, out var sourceProperty) || !ShouldShow(sourceProperty, showIfAttribute))
                return -EditorGUIUtility.standardVerticalSpacing;

            return EditorGUI.GetPropertyHeight(property, label);
        }
    }
}