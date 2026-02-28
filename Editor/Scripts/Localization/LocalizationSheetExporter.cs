#if CUSTOM_LOCALIZATION
using CustomUtils.Runtime.Formatter;
using CustomUtils.Runtime.Localization;
using UnityEngine;
using ZLinq;

namespace CustomUtils.Editor.Scripts.Localization
{
    internal static class LocalizationSheetExporter
    {
        private const char Separator = '\t';

        internal static string ExportSheet(string sheetName)
        {
            var entries = LocalizationRegistry.Instance.Entries.Values
                .Where(localizationEntry => localizationEntry.TableName == sheetName)
                .OrderBy(static localizationEntry => localizationEntry.Key);

            if (entries.Count() == 0)
            {
                Debug.LogError($"[LocalizationSheetExporter::ExportSheet] No entries found for sheet '{sheetName}'.");
                return string.Empty;
            }

            using var tsvBuilder = StringBuilderScope.Create();

            tsvBuilder.Append("GUID");
            tsvBuilder.Append(Separator);
            tsvBuilder.Append("Key");

            foreach (var language in entries.First().Translations.Keys)
            {
                tsvBuilder.Append(Separator);
                tsvBuilder.Append(language.ToString());
            }

            tsvBuilder.AppendLine();

            foreach (var entry in entries)
            {
                tsvBuilder.Append(entry.GUID);
                tsvBuilder.Append(Separator);
                tsvBuilder.Append(EscapeField(entry.Key));

                foreach (var language in entries.First().Translations.Keys)
                {
                    tsvBuilder.Append(Separator);

                    if (entry.TryGetTranslation(language, out var translation))
                        tsvBuilder.Append(EscapeField(translation));
                }

                tsvBuilder.AppendLine();
            }

            return tsvBuilder.ToString();
        }

        private static string EscapeField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            if (!field.Contains("\n") && !field.Contains("\t") && !field.Contains("\""))
                return field;

            using var escaped = StringBuilderScope.Create();
            escaped.Append('"');

            foreach (var character in field)
            {
                if (character == '"')
                    escaped.Append("\"\"");
                else
                    escaped.Append(character);
            }

            escaped.Append('"');
            return escaped.ToString();
        }
    }
}
#endif