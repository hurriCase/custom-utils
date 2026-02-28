using CustomUtils.Editor.Scripts.Extensions;
using CustomUtils.Runtime.Downloader;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.SheetsDownloader
{
    [CustomPropertyDrawer(typeof(Sheet), true)]
    public class SheetPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);
            var propertyRect = new Rect(position.x, position.y, position.width, propertyHeight);
            var buttonRect = new Rect(
                position.x,
                position.y + propertyHeight + EditorGUIUtility.standardVerticalSpacing,
                position.width,
                EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(propertyRect, property, label, true);

            if (!property.isExpanded)
                return;

            if (GUI.Button(buttonRect, "▼ Download"))
            {
                var sheetId = property.FindFieldRelative(nameof(Sheet.Id)).intValue;

                var window = EditorWindow.focusedWindow;
                if (window && window is ISheetsDownloaderWindow downloaderWindow)
                    downloaderWindow.DownloadSheet(sheetId);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var baseHeight = EditorGUI.GetPropertyHeight(property, label, true);

            if (property.isExpanded)
                return baseHeight + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            return baseHeight;
        }
    }
}