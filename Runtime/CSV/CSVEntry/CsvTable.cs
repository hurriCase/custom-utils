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
        public CsvRow[] Rows { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvTable"/> struct.
        /// </summary>
        /// <param name="rows">The data rows of the CSV document.</param>
        public CsvTable(CsvRow[] rows)
        {
            Rows = rows;
        }
    }
}