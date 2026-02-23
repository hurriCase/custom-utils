#if CUSTOM_LOCALIZATION
using UnityEngine.UIElements;

namespace CustomUtils.Editor.Scripts.Localization.LocalizationSettings
{
    internal readonly struct LocalizationSettingsElements
    {
        internal EnumField DefaultLanguageField { get; }
        internal DropdownField SheetSelectionDropdown { get; }
        internal Button ExportSheetButton { get; }
        internal DropdownField LanguageSelectionDropdown { get; }
        internal Button CopyAllTextButton { get; }

        internal LocalizationSettingsElements(VisualElement root)
        {
            DefaultLanguageField = root.Q<EnumField>("DefaultLanguageField");
            SheetSelectionDropdown = root.Q<DropdownField>("SheetSelection");
            ExportSheetButton = root.Q<Button>("ExportSheetButton");
            LanguageSelectionDropdown = root.Q<DropdownField>("LanguageSelection");
            CopyAllTextButton = root.Q<Button>("CopyAllButton");
        }
    }
}
#endif