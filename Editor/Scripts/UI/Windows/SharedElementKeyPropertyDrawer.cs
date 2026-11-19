using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using CustomUtils.Runtime.Formatter;
using CustomUtils.Runtime.UI.Windows;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.UI.Windows
{
    [CustomPropertyDrawer(typeof(SharedElementKeyAttribute))]
    internal sealed class SharedElementKeyPropertyDrawer : PropertyDrawer
    {
        private const string NoKeysMessage = "Add keys to " + nameof(SharedElementKeyDatabase) + " first.";
        private const string UnknownKeyMessage = "Key '{0}' is not in " + nameof(SharedElementKeyDatabase) + ".";

        private EditorStateControls _editorStateControls;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var rowCount = IsKnownKey(property) ? 1 : 2;
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * rowCount;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var keys = SharedElementKeyDatabase.Instance.Keys;
            if (keys.Count == 0)
            {
                EditorVisualControls.WarningBox(position, NoKeysMessage);
                return;
            }

            _editorStateControls ??=
                new EditorStateControls(property.serializedObject.targetObject, property.serializedObject);

            var dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var isKnownKey = IsKnownKey(property);
            _editorStateControls.Dropdown(property, keys, dropdownRect);

            if (isKnownKey)
                return;

            var rowOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var warningY = position.y + rowOffset;
            var warningRect = new Rect(position.x, warningY, position.width, EditorGUIUtility.singleLineHeight);
            var message = StringFormatter.Format(UnknownKeyMessage, property.stringValue);
            EditorVisualControls.WarningBox(warningRect, message);
        }

        private bool IsKnownKey(SerializedProperty property)
            => SharedElementKeyDatabase.Instance.Keys.Contains(property.stringValue);
    }
}
