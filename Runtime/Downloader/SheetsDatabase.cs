using System.Collections.Generic;
using CustomUtils.Runtime.CustomTypes.Singletons;
using CustomUtils.Runtime.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Downloader
{
    /// <inheritdoc />
    /// <summary>
    /// Abstract base class for Google Sheets database configurations that manage sheet information and download settings.
    /// Provides a singleton ScriptableObject implementation for storing Google Sheets table ID and sheet metadata.
    /// </summary>
    /// <typeparam name="TDatabase">The concrete database type that inherits from this class, used for singleton
    /// pattern implementation.</typeparam>
    /// <typeparam name="TSheet">The type of sheets to use it for a database</typeparam>
    [UsedImplicitly]
    public abstract class SheetsDatabase<TDatabase, TSheet> : SingletonScriptableObject<TDatabase>
        where TDatabase : SheetsDatabase<TDatabase, TSheet>
        where TSheet : Sheet, new()
    {
        /// <summary>
        /// Gets or sets the Google Sheets table identifier used to access the spreadsheet document.
        /// This should be the spreadsheet ID extracted from the Google Sheets URL.
        /// </summary>
        /// <value>The table ID string, or null/empty if not configured.</value>
        [UsedImplicitly]
        [field: SerializeField] public string TableId { get; set; }

        /// <summary>
        /// Gets or sets the collection of sheet metadata that represents individual sheets within the Google Sheets document.
        /// Each sheet contains information such as ID, name, content length, and associated TextAsset reference.
        /// </summary>
        /// <value>A list of <see cref="Sheet"/> objects representing the available sheets. Defaults to an empty list.</value>
        [UsedImplicitly]
        [field: SerializeField] public List<TSheet> Sheets { get; private set; } = new();

        /// <summary>
        /// Gets the file system path where downloaded sheet CSV files should be stored.
        /// Derived classes can override this method to specify custom download locations.
        /// </summary>
        /// <returns>The directory path where CSV files will be saved. Defaults to "Assets/Resources/Sheets/".</returns>
        [UsedImplicitly]
        public virtual string GetDownloadPath() => "Assets/Resources/Sheets/";

        internal void ReplaceSheets(Dictionary<string, long> sheets)
        {
            Sheets.Clear();
            foreach (var (sheetName, id) in sheets)
            {
                Sheets.Add(new TSheet
                {
                    Id = id,
                    Name = sheetName,
                    ContentLength = 0
                });
            }

            this.MarkAsDirty();
        }
    }
}