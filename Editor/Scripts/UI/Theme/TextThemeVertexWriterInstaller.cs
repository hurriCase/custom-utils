using CustomUtils.Runtime.UI.Theme;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.UI.Theme
{
    [InitializeOnLoad]
    internal static class TextThemeVertexWriterInstaller
    {
        static TextThemeVertexWriterInstaller()
        {
            ObjectFactory.componentWasAdded += AddToThemedText;
        }

        private static void AddToThemedText(Component component)
        {
            if (component is ThemeComponent && component.TryGetComponent<TMP_Text>(out _))
                Undo.AddComponent<TextThemeVertexWriter>(component.gameObject);
        }
    }
}
