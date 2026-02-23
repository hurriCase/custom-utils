#if CUSTOM_LOCALIZATION
using UnityEngine.UIElements;

namespace CustomUtils.Editor.Scripts.Localization.LocalizationSelector
{
    internal readonly struct LocalizationSelectorElements
    {
        internal DropdownField TableNamesDropdown { get; }
        internal TextField SearchTextField { get; }
        internal ListView LocalizationEntriesList { get; }
        internal Label NoneSelectedLabel { get; }
        internal VisualElement CurrentSelectionInfo { get; }
        internal Label KeyLabel { get; }
        internal Label TableNameLabel { get; }
        internal Label GUIDLabel { get; }
        internal Label LocalizationLabel { get; }

        internal LocalizationSelectorElements(VisualElement root)
        {
            TableNamesDropdown = root.Q<DropdownField>("TableSelection");
            SearchTextField = root.Q<TextField>("LocalizationKeySearch");
            LocalizationEntriesList = root.Q<ListView>("EntriesList");
            NoneSelectedLabel = root.Q<Label>("NoneSelectedLabel");
            CurrentSelectionInfo = root.Q<VisualElement>("CurrentSelectionInfo");
            KeyLabel = root.Q<Label>("KeyLabel");
            TableNameLabel = root.Q<Label>("TableNameLabel");
            GUIDLabel = root.Q<Label>("GUIDLabel");
            LocalizationLabel = root.Q<Label>("LocalizationLabel");
        }
    }
}
#endif