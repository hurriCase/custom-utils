using CustomUtils.Editor.Scripts.FigmaBridge.Clipboard;
using CustomUtils.Editor.Scripts.FigmaBridge.Colors.Data;
using CustomUtils.Runtime.UI.Theme;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Colors
{
    [InitializeOnLoad]
    internal static class ThemeStyleFigmaClipboardMenu
    {
        private const string PasteMenuName = "Paste Color From Figma";
        private const char PathSeparator = '.';

        static ThemeStyleFigmaClipboardMenu()
        {
            EditorApplication.contextualPropertyMenu += AddPasteItem;
        }

        private static void AddPasteItem(GenericMenu menu, SerializedProperty property)
        {
            // Any field inside a style (color data, rotation, halftone, ...) pastes into the whole style
            var styleProperty = FindStyleProperty(property);
            if (styleProperty == null)
                return;

            var content = new GUIContent(PasteMenuName);
            menu.AddItem(content, false, static data => Paste((SerializedProperty)data), styleProperty);
        }

        private static SerializedProperty FindStyleProperty(SerializedProperty property)
        {
            var path = property.propertyPath;
            while (true)
            {
                var candidate = property.serializedObject.FindProperty(path);
                if (candidate is { type: nameof(ThemeStyle) })
                    return candidate;

                var separatorIndex = path.LastIndexOf(PathSeparator);
                if (separatorIndex < 0)
                    return null;

                path = path.Substring(0, separatorIndex);
            }
        }

        private static void Paste(SerializedProperty property)
        {
            var targetName = property.displayName;
            if (!FigmaClipboardReader.TryRead<ColorFigmaClipboardData>(
                    ColorFigmaClipboardData.ExpectedSource, targetName, out var clipboardData))
                return;

            property.serializedObject.Update();
            var currentStyle = (ThemeStyle)property.boxedValue;
            property.boxedValue = ThemeStyleFigmaFactory.Create(clipboardData, currentStyle);
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
