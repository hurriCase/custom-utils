using CustomUtils.Runtime.CSV.CSVEntry;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.CSV.Base
{
    /// <summary>
    /// Defines a contract for converting CSV table data into strongly-typed objects.
    /// </summary>
    /// <typeparam name="T">The type of objects to convert CSV rows into.</typeparam>
    [PublicAPI]
    public interface ICsvConverter<out T>
    {
        /// <summary>
        /// Converts all rows in a CSV table to an array of strongly-typed objects.
        /// </summary>
        /// <param name="csvTable">The CSV table containing the data to convert.</param>
        /// <returns>An array of objects of type T, one for each row in the CSV table.</returns>
        T[] ConvertToObjects(CsvTable csvTable);
    }
}