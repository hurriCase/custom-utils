namespace CustomUtils.Editor.Scripts.Localization
{
    internal static class LocalizationConstants
    {
        internal const string SheetsValueName = "sheets";
        internal const string LanguagesValueName = "languages";

        internal const string NoChoiceMessageFormat = "No {0} available";
        internal const string NoSheetsAvailable = "No sheets available";
        internal const string NoLanguagesAvailable = "No languages available";

        internal const string SelectSheetFirstMessage = "Please select a sheet first";
        internal const string SelectLanguageFirstMessage = "Please select a language first";
        internal const string InvalidLanguageSelectionMessage = "Invalid language selection";
        internal const string NoLocalizationEntriesMessage = "No localization entries found";
        internal const string NoTranslationsFoundFormat = "No translations found for ";

        internal const string ExportFailedMessageFormat = "Failed to export sheet '{0}'. Make sure the sheet is loaded";
        internal const string ExportSuccessMessageFormat =
            "Exported sheet '{0}' to CSV and copied to clipboard.\n\nYou can now paste this into your Google Sheet";
        internal const string CopySuccessMessageFormat = "Copied {0} text entries for {1} to clipboard";

        internal const string LanguageIconPath = "Packages/com.firsttry.customutils/Editor/Icons/language_icon.png";
        internal const string ToolbarPath = "Localization/Change Language";
    }
}