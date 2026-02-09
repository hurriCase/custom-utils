using CustomUtils.Runtime.Localization;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.Localization
{
    [UsedImplicitly]
    internal sealed class LanguageSwitchToolbar
    {
        [UsedImplicitly]
        [MainToolbarElement(LocalizationConstants.ToolbarPath, defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarDropdown LanguageSwitchButton()
        {
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(LocalizationConstants.LanguageIconPath);
            var content = new MainToolbarContent(icon);

            return new MainToolbarDropdown(content, static rect =>
            {
                var menu = new GenericMenu();
                var supportedLanguages = LocalizationRegistry.Instance.SupportedLanguages;
                var currentLanguage = LocalizationController.Language.Value;

                foreach (var language in supportedLanguages)
                {
                    var isSelected = language == currentLanguage;
                    menu.AddItem(new GUIContent(language.ToString()), isSelected,
                        () => LocalizationController.Language.Value = language);
                }

                menu.DropDown(rect);
            });
        }
    }
}