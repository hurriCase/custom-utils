using JetBrains.Annotations;

namespace CustomUtils.Runtime.CSV.CSVEntry
{
    /// <summary>
    /// Represents a parsed CSV document containing an array of rows.
    /// </summary>
    [PublicAPI]
    public readonly struct CsvTable
    {
        /// <summary>
        /// Gets the collection of rows in this CSV document.
        /// </summary>
        internal CsvRow[] Rows { get; }

        internal CsvTable(CsvRow[] rows)
        {
            Rows = rows;
        }
    }
}