#if CUSTOM_LOCALIZATION
using AYellowpaper.SerializedCollections;
using CustomUtils.Editor.Scripts.Localization.LocalizationSelector;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Localization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CustomUtils.Editor.Scripts.Localization.KeyDrawer
{
    internal sealed class LocalizationKeyElement : VisualElement
    {
        private const string NoneValue = "[None]";

        private SerializedDictionary<string, LocalizationEntry> Entries => LocalizationRegistry.Instance.Entries;

        private readonly SerializedProperty _guidProperty;
        private readonly Foldout _translationsContainer;
        private readonly DropdownField _keyDropdown;

        private LocalizationEntry _selectedEntry;

        internal LocalizationKeyElement(SerializedProperty guidProperty, string label)
        {
            _guidProperty = guidProperty;
            _translationsContainer = new Foldout { text = label };
            _keyDropdown = CreateKeyDropdown(label);

            SetupFoldoutToggle();
            Add(_translationsContainer);

            OnPropertyChanged(_guidProperty);
            this.TrackPropertyValue(_guidProperty, OnPropertyChanged);
        }

        private void OnPropertyChanged(SerializedProperty property)
        {
            if (Entries.TryGetValue(property.stringValue, out var entry))
                UpdateLocalizationKey(entry);
        }

        private void ChangeLocalizationKey(LocalizationEntry selectedEntry)
        {
            Undo.RecordObject(_guidProperty.serializedObject.targetObject, "Change Localization Key");

            _guidProperty.stringValue = selectedEntry.GUID;
            _guidProperty.serializedObject.ApplyModifiedProperties();

            UpdateLocalizationKey(selectedEntry);
        }

        private void UpdateLocalizationKey(LocalizationEntry entry)
        {
            _selectedEntry = entry;
            _keyDropdown.value = entry.Key;

            DisplayTranslations(entry);
        }

        private void DisplayTranslations(LocalizationEntry entry)
        {
            _translationsContainer.Clear();

            foreach (var (systemLanguage, localization) in entry.Translations)
            {
                var translation = new TextField
                {
                    isReadOnly = true,
                    label = systemLanguage.ToString(),
                    value = localization
                };

                _translationsContainer.Add(translation);
            }
        }

        private void ShowKeySelectionWindow()
        {
            LocalizationSelectorWindow.ShowWindow(_selectedEntry, ChangeLocalizationKey);
        }

        private void SetupFoldoutToggle()
        {
            if (!_translationsContainer.TryQ(out Foldout foldout))
                return;

            foldout.value = false;

            if (!foldout.TryQ(out Toggle toggle))
                return;

            if (toggle.TryQ(out Label toggleLabel))
                toggleLabel.RemoveFromHierarchy();

            if (toggle.TryQ(out VisualElement toggleInput, className: Toggle.inputUssClassName))
                toggleInput.Add(_keyDropdown);
        }

        private DropdownField CreateKeyDropdown(string label)
        {
            var dropdown = new DropdownField
            {
                label = label,
                value = NoneValue,
                style = { marginLeft = 0 }
            };

            dropdown.AddUnityFileStyles();
            dropdown.RegisterInputClick(this, static self => self.ShowKeySelectionWindow());

            SetupDropdownLabel(dropdown);

            return dropdown;
        }

        private void SetupDropdownLabel(VisualElement dropdown)
        {
            if (!dropdown.TryQ(out Label fieldLabel))
                return;

            fieldLabel.style.marginLeft = 0;
            fieldLabel.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                AppendCopyAction(evt);
                AppendPasteAction(evt);
            }));
        }

        private void AppendCopyAction(ContextualMenuPopulateEvent menuEvent)
        {
            var copyState = _selectedEntry != null
                ? DropdownMenuAction.Status.Normal
                : DropdownMenuAction.Status.Disabled;

            menuEvent.menu.AppendAction("Copy Localization Key", CopyGUID, copyState);
        }

        private void AppendPasteAction(ContextualMenuPopulateEvent menuEvent)
        {
            var clipboardContent = EditorGUIUtility.systemCopyBuffer;
            var pasteState = Entries.ContainsKey(clipboardContent)
                ? DropdownMenuAction.Status.Normal
                : DropdownMenuAction.Status.Disabled;

            menuEvent.menu.AppendAction("Paste Localization Key", PasteLocalizationKey, pasteState);
        }

        private void CopyGUID(DropdownMenuAction _)
        {
            EditorGUIUtility.systemCopyBuffer = _selectedEntry?.GUID;
        }

        private void PasteLocalizationKey(DropdownMenuAction _)
        {
            var clipboardContent = EditorGUIUtility.systemCopyBuffer;

            if (string.IsNullOrEmpty(clipboardContent))
            {
                ChangeLocalizationKey(null);
                return;
            }

            if (Entries.TryGetValue(clipboardContent, out var entry))
                ChangeLocalizationKey(entry);
        }
    }
}
#endif