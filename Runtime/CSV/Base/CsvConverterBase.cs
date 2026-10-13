using CustomUtils.Runtime.CSV.CSVEntry;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.CSV.Base
{
    /// <inheritdoc />
    /// <summary>
    /// Base <see cref="T:CustomUtils.Runtime.CSV.Base.ICsvConverter`1">ICsvConverter{T}</see>
    /// that converts a table row by row via <see cref="M:CustomUtils.Runtime.CSV.Base.CsvConverterBase`1.ConvertRow(CustomUtils.Runtime.CSV.CSVEntry.CsvRow)">ConvertRow</see>.
    /// </summary>
    /// <typeparam name="T">The type of objects to convert CSV rows into.</typeparam>
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
        /// Converts a single <see cref="CsvRow"/> into a <typeparamref name="T"/> object.
        /// </summary>
        /// <param name="row">The CSV row to convert.</param>
        /// <returns>A <typeparamref name="T"/> object created from the row data.</returns>
        protected abstract T ConvertRow(CsvRow row);
    }
}