#if CUSTOM_LOCALIZATION
using System;
using System.Collections.Generic;
using CustomUtils.Editor.Scripts.SheetsDownloader;
using CustomUtils.Runtime.Downloader;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Formatter;
using CustomUtils.Runtime.Localization;
using CustomUtils.Runtime.ResponseTypes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ZLinq;

namespace CustomUtils.Editor.Scripts.Localization.LocalizationSettings
{
    internal sealed class LocalizationSettingsWindow : SheetsDownloaderWindowBase<LocalizationDatabase, Sheet>
    {
        [SerializeField] private VisualTreeAsset _customLayout;

        protected override LocalizationDatabase Database => LocalizationDatabase.Instance;
        private LocalizationRegistry Registry => LocalizationRegistry.Instance;

        private LocalizationSettingsElements _elements;

        [MenuItem(MenuItemNames.LocalizationMenuName)]
        internal static void ShowWindow()
        {
            GetWindow<LocalizationSettingsWindow>(nameof(LocalizationSettingsWindow).ToSpacedWords());
        }

        protected override void OnSheetsDownloaded()
        {
            LocalizationController.ReadLocalizationData();

            UpdateSheetsDisplay();
            UpdateLanguagesDisplay();
        }

        protected override void CreateCustomContent(VisualElement parent)
        {
            _customLayout.CloneTree(parent);
            _elements = new LocalizationSettingsElements(parent);

            SetupDefaultLanguageField();
            SetupSheetExportSection();
            SetupCopyAllTextSection();
        }

        private void SetupDefaultLanguageField()
        {
            _elements.DefaultLanguageField.Init(Database.DefaultLanguage);
            _elements.DefaultLanguageField.value = Database.DefaultLanguage;

            _elements.DefaultLanguageField.RegisterValueChangedCallback(changeEvent =>
            {
                Database.DefaultLanguage = (SystemLanguage)changeEvent.newValue;
                EditorUtility.SetDirty(Database);
            });
        }

        private void SetupSheetExportSection()
        {
            UpdateSheetsDisplay();

            _elements.ExportSheetButton.clicked += () =>
            {
                var result = ExportSheet();
                result.DisplayMessage();
            };
        }

        private void SetupCopyAllTextSection()
        {
            UpdateLanguagesDisplay();

            _elements.CopyAllTextButton.clicked += () =>
            {
                var result = CopyAllTextForLanguage();
                result.DisplayMessage();
            };
        }

        private void UpdateSheetsDisplay()
        {
            var choiceNames = Database.Sheets.Select(static choice => choice.Name.ToString()).ToList();
            UpdateChoices(_elements.SheetSelectionDropdown, choiceNames, LocalizationConstants.SheetsValueName);
        }

        private void UpdateLanguagesDisplay()
        {
            var choiceNames = Registry.SupportedLanguages.Select(static choice => choice.ToString()).ToList();
            UpdateChoices(_elements.LanguageSelectionDropdown, choiceNames, LocalizationConstants.LanguagesValueName);
        }

        private void UpdateChoices(DropdownField dropdownField, List<string> choiceNames, string valueName)
        {
            if (choiceNames.Count == 0)
            {
                var noChoiceMessage = StringFormatter.Format(LocalizationConstants.NoChoiceMessageFormat, valueName);
                dropdownField.choices = new List<string> { noChoiceMessage };
                dropdownField.value = noChoiceMessage;
                return;
            }

            dropdownField.choices = choiceNames;

            if (choiceNames.Count > 0)
                dropdownField.value = choiceNames[0];
        }

        private Result ExportSheet()
        {
            var selectedSheet = _elements.SheetSelectionDropdown.value;

            if (string.IsNullOrEmpty(selectedSheet) || selectedSheet == LocalizationConstants.NoSheetsAvailable)
                return Result.Invalid(LocalizationConstants.SelectSheetFirstMessage);

            var csvContent = LocalizationSheetExporter.ExportSheet(selectedSheet);

            if (string.IsNullOrEmpty(csvContent))
            {
                var errorMessage = StringFormatter.Format(LocalizationConstants.ExportFailedMessageFormat, selectedSheet);
                return Result.Invalid(errorMessage);
            }

            EditorGUIUtility.systemCopyBuffer = csvContent;
            var successMessage = StringFormatter.Format(LocalizationConstants.ExportSuccessMessageFormat, selectedSheet);
            return Result.Valid(successMessage);
        }

        private Result CopyAllTextForLanguage()
        {
            var selectedLanguageString = _elements.LanguageSelectionDropdown.value;

            if (string.IsNullOrEmpty(selectedLanguageString) ||
                selectedLanguageString == LocalizationConstants.NoLanguagesAvailable)
                return Result.Invalid(LocalizationConstants.SelectLanguageFirstMessage);

            if (Enum.TryParse<SystemLanguage>(selectedLanguageString, out var language) is false)
                return Result.Invalid(LocalizationConstants.InvalidLanguageSelectionMessage);

            var allEntries = Registry.Entries.Values;

            if (allEntries.Count == 0)
                return Result.Invalid(LocalizationConstants.NoLocalizationEntriesMessage);

            using var textBuilder = StringBuilderScope.Create();
            var copiedCount = 0;

            foreach (var entry in allEntries)
            {
                if (entry.TryGetTranslation(language, out var localizedText) is false ||
                    string.IsNullOrEmpty(localizedText))
                    continue;

                textBuilder.AppendLine(localizedText);

                copiedCount++;
            }

            if (copiedCount == 0)
                return Result.Invalid(StringFormatter.Concat(LocalizationConstants.NoTranslationsFoundFormat,
                    language));

            EditorGUIUtility.systemCopyBuffer = textBuilder.ToString();
            return Result.Valid(StringFormatter.Format(LocalizationConstants.CopySuccessMessageFormat,
                copiedCount, language));
        }
    }
}
#endif