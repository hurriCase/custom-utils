using CustomUtils.Runtime.CSV.CSVEntry;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.CSV.Base
{
    /// <inheritdoc />
    [PublicAPI]
    public abstract class CsvConverterBase<T> : ICsvConverter<T>
    {
        /// <inheritdoc />
        public T[] ConvertToObjects(CsvTable csvTable)
        {
            var objects = new T[csvTable.Rows.Length];

            for (var i = 0; i < csvTable.Rows.Length; i++)
                objects[i] = ConvertRow(csvTable.Rows[i]);

            return objects;
        }

        /// <summary>
        /// Converts a single CSV row to an object of type T.
        /// This method must be implemented by derived classes to define the conversion logic.
        /// </summary>
        /// <param name="row">The CSV row to convert.</param>
        /// <returns>An object of type T created from the row data.</returns>
        protected abstract T ConvertRow(CsvRow row);
    }
}