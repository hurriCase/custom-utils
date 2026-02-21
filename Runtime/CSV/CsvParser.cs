using System;
using System.Collections.Generic;
using CustomUtils.Runtime.CSV.CSVEntry;
using CustomUtils.Runtime.Formatter;

namespace CustomUtils.Runtime.CSV
{
    internal static class CsvParser
    {
        private const char Quote = '"';
        private const char Comma = ',';

        internal static CsvTable Parse(string csvContent)
        {
            if (string.IsNullOrWhiteSpace(csvContent))
                return new CsvTable(Array.Empty<CsvRow>());

            csvContent = csvContent.Replace("\r\n", "\n");

            var lines = SplitIntoLines(csvContent);

            if (lines.Count < 2)
                return new CsvTable(Array.Empty<CsvRow>());

            var header = ParseLine(lines[0]);
            var rows = new CsvRow[lines.Count - 1];

            for (var i = 1; i < lines.Count; i++)
            {
                var fields = ParseLine(lines[i]);
                rows[i - 1] = new CsvRow(fields, header);
            }

            return new CsvTable(rows);
        }

        private static List<string> SplitIntoLines(string content)
        {
            var lines = new List<string>();
            var inQuotes = false;
            var lineStart = 0;

            for (var i = 0; i < content.Length; i++)
            {
                if (content[i] == Quote)
                    inQuotes = !inQuotes;

                if (content[i] != '\n' || inQuotes is true)
                    continue;

                var line = content.Substring(lineStart, i - lineStart).Trim();
                if (string.IsNullOrWhiteSpace(line) is false)
                    lines.Add(line);

                lineStart = i + 1;
            }

            if (lineStart >= content.Length)
                return lines;

            var lastLine = content[lineStart..].Trim();
            if (string.IsNullOrWhiteSpace(lastLine) is false)
                lines.Add(lastLine);

            return lines;
        }

        private static string[] ParseLine(string line)
        {
            var fields = new List<string>();
            var inQuotes = false;

            using var fieldBuilder = StringBuilderScope.Create();

            for (var i = 0; i < line.Length; i++)
            {
                var current = line[i];

                switch (current)
                {
                    case Quote:
                    {
                        var next = i + 1 < line.Length ? line[i + 1] : '\0';

                        if (inQuotes && next == Quote)
                        {
                            fieldBuilder.Append(Quote);
                            i++; // Skip escaped quote
                        }
                        else
                            inQuotes = !inQuotes;

                        break;
                    }
                    case Comma when inQuotes is false:
                        fields.Add(fieldBuilder.ToString().Trim());
                        fieldBuilder.Clear();
                        break;
                    default:
                        fieldBuilder.Append(current);
                        break;
                }
            }

            fields.Add(fieldBuilder.ToString().Trim());

            return fields.ToArray();
        }
    }
}