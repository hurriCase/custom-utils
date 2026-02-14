using CustomUtils.Editor.Scripts.Extensions;
using CustomUtils.Editor.Scripts.Localization.LocalizationSelector;
using CustomUtils.Runtime.Localization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUtils.Editor.Scripts.Localization.KeyDrawer
{
    [CustomPropertyDrawer(typeof(LocalizationKey))]
    internal sealed class LocalizationKeyDrawer : PropertyDrawer
    {
        [SerializeField] private StyleSheet _unityFieldStyle;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var guidProperty = property.FindFieldRelative(nameof(LocalizationKey.GUID));
            var rootVisualElement = new LocalizationKeyElement(guidProperty, preferredLabel);

            rootVisualElement.styleSheets.Add(_unityFieldStyle);

            return rootVisualElement;
        }

        // Simplified version for embedded editor cases
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var guidProperty = property.FindFieldRelative(nameof(LocalizationKey.GUID));
            var currentGuid = guidProperty.stringValue;

            var rect = EditorGUI.PrefixLabel(position, label);

            var currentKey = LocalizationRegistry.Instance.Entries.TryGetValue(currentGuid, out var entry)
                ? entry.Key
                : "[None]";

            if (GUI.Button(rect, currentKey, EditorStyles.popup))
                LocalizationSelectorWindow.ShowWindow(entry, selectedEntry =>
                {
                    guidProperty.stringValue = selectedEntry.GUID;
                    guidProperty.serializedObject.ApplyModifiedProperties();
                });
        }
    }
}