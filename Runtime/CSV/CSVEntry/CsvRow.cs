using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.CSV.CSVEntry
{
    /// <summary>
    /// Represents a single row in a CSV document with column-based value access.
    /// </summary>
    [PublicAPI]
    public readonly struct CsvRow
    {
        private readonly Dictionary<string, int> _columnMap;
        private readonly string[] _values;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvRow"/> struct.
        /// </summary>
        /// <param name="values">The field values of the row, ordered by column.</param>
        /// <param name="columnNames">The header column names, matched case-insensitively on lookup.</param>
        public CsvRow(string[] values, IReadOnlyList<string> columnNames)
        {
            _values = values;

            _columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < columnNames.Count; i++)
                _columnMap[columnNames[i]] = i;
        }

        /// <summary>
        /// Attempts to get the value of the specified column in this row.
        /// </summary>
        /// <param name="columnName">The column name to look up (case-insensitive).</param>
        /// <param name="value">When this method returns,
        /// contains the value of the specified column if it was found; otherwise, an empty string.</param>
        /// <returns><c>true</c> if the value was found; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(string columnName, out string value)
        {
            value = string.Empty;
            if (!_columnMap.TryGetValue(columnName, out var index) || index >= _values.Length)
                return false;

            value = _values[index];
            return value != null;
        }

        /// <summary>
        /// Gets the value of the specified column in this row.
        /// </summary>
        /// <param name="columnName">The column name to look up (case-insensitive).</param>
        /// <returns>The value of the specified column, or an empty string if the column is not found.</returns>
        public string GetValue(string columnName)
        {
            if (_columnMap.TryGetValue(columnName, out var index) && index < _values.Length)
                return _values[index] ?? string.Empty;

            return string.Empty;
        }

        /// <summary>
        /// Gets the value of the first column whose name matches the specified regex pattern.
        /// </summary>
        /// <param name="pattern">The regex pattern to match against column names (case-insensitive).</param>
        /// <returns>The first matching value, or an empty string if no column matches the pattern.</returns>
        public string GetValueByPattern(string pattern)
        {
            foreach (var (headerName, index) in _columnMap)
            {
                if (!Regex.IsMatch(headerName, pattern, RegexOptions.IgnoreCase))
                    continue;

                if (index < _values.Length)
                    return _values[index] ?? string.Empty;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the values of all columns whose names match the specified regex pattern.
        /// </summary>
        /// <param name="pattern">The regex pattern to match against column names (case-insensitive).</param>
        /// <returns>The non-empty values of all matching columns; an empty list if none match.</returns>
        public List<string> GetValuesByPattern(string pattern)
        {
            var result = new List<string>();

            foreach (var (headerName, index) in _columnMap)
            {
                if (!Regex.IsMatch(headerName, pattern, RegexOptions.IgnoreCase))
                    continue;

                if (index < _values.Length && !string.IsNullOrEmpty(_values[index]))
                    result.Add(_values[index]);
            }

            return result;
        }
    }
}