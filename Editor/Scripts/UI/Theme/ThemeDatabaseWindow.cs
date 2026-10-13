using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.Theme;
using CustomUtils.Runtime.UI.Theme.Databases;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUtils.Editor.Scripts.UI.Theme
{
    internal sealed class ThemeDatabaseWindow : EditorWindow
    {
        [MenuItem(MenuItemNames.ThemeDatabasesMenuName)]
        private static void OpenWindow()
        {
            GetWindow<ThemeDatabaseWindow>(nameof(ThemeDatabaseWindow).ToSpacedWords());
        }

        private void CreateGUI()
        {
            var toolbar = new Toolbar();

            var config = ThemeDatabaseConfig.Instance;
            toolbar.Add(new ToolbarButton(() => ShowDatabase(config.Solid)) { text = nameof(ColorType.Solid) });
            toolbar.Add(new ToolbarButton(() => ShowDatabase(config.Gradient)) { text = nameof(ColorType.Gradient) });

            rootVisualElement.Add(toolbar);
            rootVisualElement.Add(new ScrollView { name = "tab-content" });

            ShowDatabase(config.Solid);
        }

        private void ShowDatabase(Object database)
        {
            var scrollView = rootVisualElement.Q<ScrollView>("tab-content");
            scrollView.Clear();
            scrollView.Add(new InspectorElement(database));
        }
    }
}