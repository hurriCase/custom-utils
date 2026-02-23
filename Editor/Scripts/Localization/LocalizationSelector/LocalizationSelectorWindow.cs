#if CUSTOM_LOCALIZATION
using System;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Localization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUtils.Editor.Scripts.Localization.LocalizationSelector
{
    internal sealed class LocalizationSelectorWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset _windowLayout;

        private const string AllTablesName = "all tables";
        private const string TableNameKey = "tableName";

        private LocalizationSelectorElements _elements;
        private LocalizationEntry _currentLocalizationEntry;
        private Action<LocalizationEntry> _onSelectionChanged;

        internal static void ShowWindow(LocalizationEntry entry, Action<LocalizationEntry> onSelectionChanged)
        {
            var window = CreateInstance<LocalizationSelectorWindow>();

            window.titleContent = new GUIContent("Localization Selector");
            window._currentLocalizationEntry = entry;
            window._onSelectionChanged = onSelectionChanged;

            window.ShowUtility();
        }

        private void CreateGUI()
        {
            _windowLayout.CloneTree(rootVisualElement);
            _elements = new LocalizationSelectorElements(rootVisualElement);

            SetupTableNamesDropdown();

            _elements.SearchTextField.RegisterValueChangedCallback(OnSearchChanged);
            _elements.SearchTextField.Focus();

            SetupCurrentSelection();
            SetupLocalizationEntriesList();
        }

        private void SetupTableNamesDropdown()
        {
            var tableNames = LocalizationRegistry.Instance.TableToGuids.Keys.ToList();
            tableNames.Insert(0, AllTablesName);

            _elements.TableNamesDropdown.choices = tableNames;
            _elements.TableNamesDropdown.value = EditorPrefs.GetString(TableNameKey, AllTablesName);;
            _elements.TableNamesDropdown.RegisterValueChangedCallback(OnSearchChanged);
        }

        private void SetupCurrentSelection()
        {
            if (_currentLocalizationEntry is null)
            {
                _elements.NoneSelectedLabel.SetActive(true);
                _elements.CurrentSelectionInfo.SetActive(false);
                return;
            }

            var isValid = _currentLocalizationEntry.IsValid;
            _elements.NoneSelectedLabel.SetActive(isValid is false);
            _elements.CurrentSelectionInfo.SetActive(isValid);

            if (isValid is false)
                return;

            DisplayCurrentEntry();
        }

        private void DisplayCurrentEntry()
        {
            var tableName = nameof(_currentLocalizationEntry.TableName).ToSpacedWords();
            _elements.TableNameLabel.text = $"{tableName}: {_currentLocalizationEntry.TableName}";
            _elements.KeyLabel.text = $"{nameof(_currentLocalizationEntry.Key)}: {_currentLocalizationEntry.Key}";
            _elements.GUIDLabel.text = $"{nameof(_currentLocalizationEntry.GUID)}: {_currentLocalizationEntry.GUID}";

            var englishTranslation = _currentLocalizationEntry.Translations[SystemLanguage.English];
            _elements.LocalizationLabel.text = englishTranslation;
        }

        private void SetupLocalizationEntriesList()
        {
            _elements.LocalizationEntriesList.itemsSource = LocalizationRegistry.Instance.SearchEntries(string.Empty);
            _elements.LocalizationEntriesList.bindItem = BindItem;
        }

        private void OnSearchChanged(ChangeEvent<string> changeEvent)
        {
            var tableName = _elements.TableNamesDropdown.value == AllTablesName
                ? null
                : _elements.TableNamesDropdown.value;

            EditorPrefs.SetString(TableNameKey, _elements.TableNamesDropdown.value);

            _elements.LocalizationEntriesList.itemsSource = LocalizationRegistry.Instance.SearchEntries(
                _elements.SearchTextField.value,
                tableName);
        }

        private void BindItem(VisualElement element, int index)
        {
            var entry = (LocalizationEntry)_elements.LocalizationEntriesList.itemsSource[index];

            element.Q<Label>("KeyLabel").text = entry.Key;
            element.Q<Label>("TableNameLabel").text = entry.TableName;

            var englishTranslation = entry.Translations[SystemLanguage.English];
            element.Q<Label>("LocalizationLabel").text = englishTranslation;

            element.Q<Button>("SelectButton").clicked += () => OnSelectClicked(entry);
        }

        private void OnSelectClicked(LocalizationEntry localizationEntry)
        {
            _currentLocalizationEntry = localizationEntry;
            SetupCurrentSelection();
            _onSelectionChanged?.Invoke(localizationEntry);

            Close();
        }
    }
}
#endif