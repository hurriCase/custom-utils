using CustomUtils.Runtime.CSV.CSVEntry;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.CSV.Base
{
    /// <summary>
    /// Defines a contract for converting a <see cref="CsvTable"/> into strongly-typed objects.
    /// </summary>
    /// <typeparam name="T">The type of objects to convert CSV rows into.</typeparam>
    [PublicAPI]
    public interface ICsvConverter<out T>
    {
        /// <summary>
        /// Converts all rows of a <see cref="CsvTable"/> into <typeparamref name="T"/> objects.
        /// </summary>
        /// <param name="csvTable">The CSV table containing the data to convert.</param>
        /// <returns>An array of <typeparamref name="T"/> objects, one per row in <paramref name="csvTable"/>.</returns>
        T[] ConvertToObjects(CsvTable csvTable);
    }
}