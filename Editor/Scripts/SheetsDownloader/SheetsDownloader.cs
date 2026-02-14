using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using CustomUtils.Runtime.Downloader;
using CustomUtils.Runtime.ResponseTypes;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace CustomUtils.Editor.Scripts.SheetsDownloader
{
    /// <summary>
    /// Handles downloading and managing Google Sheets data for a specified database type.
    /// Provides functionality to download sheets as CSV files and resolve sheet information
    /// from Google Sheets documents.
    /// </summary>
    /// <typeparam name="TDatabase">The type of sheet database that inherits from <see cref="SheetsDatabase{T, T}"/>
    /// .</typeparam>
    /// <typeparam name="TSheet">The type of sheets to use it for a database</typeparam>
    [UsedImplicitly]
    public sealed class SheetsDownloader<TDatabase, TSheet> where TDatabase : SheetsDatabase<TDatabase, TSheet>
        where TSheet : Sheet, new()
    {
        private readonly TDatabase _database;
        private readonly List<TSheet> _sheetsToDownload = new();

        private string RequestUrl => ZString.Format(
            SheetDownloaderConstants.RequestUrlFormat,
            SheetDownloaderConstants.SheetResolverUrl,
            _database.TableId);

        /// <summary>
        /// Initializes a new instance of the <see cref="SheetsDownloader{T, T}"/> class.
        /// </summary>
        /// <param name="database">The database instance that contains sheet configuration
        /// and will store downloaded data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="database"/> is null.</exception>
        [UsedImplicitly]
        public SheetsDownloader(TDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Resolves and updates the list of available sheets from a Google Sheets document.
        /// This method queries the Google Sheets document to get current sheet names and IDs,
        /// then updates the database's sheet collection accordingly.
        /// </summary>
        /// <returns>A <see cref="UniTask"/> representing the asynchronous resolve operation.</returns>
        /// <exception cref="Exception">Thrown when network errors occur, when the table is not found,
        /// when public read permission is not set, or when the response cannot be parsed.</exception>
        [UsedImplicitly]
        public async UniTask<Result> TryResolveGoogleSheetsAsync()
        {
            using var request = UnityWebRequest.Get(RequestUrl);

            await request.SendWebRequest().ToUniTask();

            return string.IsNullOrEmpty(request.error)
                ? ProcessResolveResponse(request)
                : Result.Invalid(ZString.Format(SheetDownloaderConstants.NetworkErrorFormat, request.error));
        }

        /// <summary>
        /// Downloads all sheets that have changed since the last download operation.
        /// Only downloads sheets whose content length differs from the cached value.
        /// </summary>
        /// <returns>A <see cref="UniTask{DownloadResult}"/> representing the asynchronous download operation.
        /// The result contains information about how many sheets were downloaded and a status message.</returns>
        /// <exception cref="ArgumentException">Thrown when the database's TableId is null or empty.</exception>
        /// <exception cref="Exception">Thrown when network errors occur
        /// or when access to the Google Sheets document is denied.</exception>
        [UsedImplicitly]
        public async UniTask<Result> DownloadSheetsAsync(CancellationToken token)
        {
            Debug.Log("[SheetsDownloader::DownloadSheetsAsync] Start downloading sheets ...");

            PrepareDownloadFolderIfNeeded();

            await FillSheetsToDownloadAsync(token);

            if (_sheetsToDownload.Count == 0)
                return Result.Invalid(SheetDownloaderConstants.AllSheetsUpToDateMessage);

            var changedCount = 0;
            foreach (var sheet in _sheetsToDownload)
            {
                Debug.Log($"[SheetsDownloader::DownloadSheetsAsync] Downloading sheet {sheet.Name}...");
                var downloadResult = await DownloadSingleSheetAsync(sheet, token);

                if (downloadResult.IsValid)
                    changedCount++;
            }

            var changeResult = changedCount > 0
                ? Result.Valid(ZString.Format(SheetDownloaderConstants.ChangedSheetsDownloadedFormat, changedCount))
                : Result.Invalid(SheetDownloaderConstants.AllSheetsUpToDateMessage);

            return changeResult;
        }

        /// <summary>
        /// Downloads a specific sheet regardless of whether it has changed.
        /// Forces download of the specified sheet and updates its content length.
        /// </summary>
        /// <param name="sheet">The sheet to download.</param>
        /// <param name="token"></param>
        /// <returns>A <see cref="UniTask{DownloadResult}"/> representing the asynchronous download operation.
        /// The result contains information about the download and a status message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sheet"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the database's TableId is null or empty.</exception>
        /// <exception cref="Exception">Thrown when network errors occur
        /// or when access to the Google Sheets document is denied.</exception>
        [UsedImplicitly]
        public async UniTask<Result> DownloadSingleSheetAsync(TSheet sheet, CancellationToken token)
        {
            if (sheet == null)
                return Result.Valid(SheetDownloaderConstants.SheetIsNullMessage);

            PrepareDownloadFolderIfNeeded();

            try
            {
                var result = await GetSheetContent(sheet.Id);

                if (!result.IsValid)
                    return Result.Invalid(result.Message);

                sheet.TextAsset = await SaveSheetDataAsync(sheet, result.Data, token);
                sheet.ContentLength = result.Data.Length;

                var successMessage = ZString.Format(SheetDownloaderConstants.SheetDownloadedSuccessFormat, sheet.Name);
                return Result.Valid(successMessage);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                var errorMessage = ZString.Format(
                    SheetDownloaderConstants.SheetDownloadFailedFormat,
                    sheet.Name,
                    ex.Message);

                return Result.Invalid(errorMessage);
            }
        }

        private async UniTask<Result<byte[]>> GetSheetContent(long id)
        {
            var url = ZString.Format(SheetDownloaderConstants.UrlPattern, _database.TableId, id);

            using var request = UnityWebRequest.Get(url);
            await request.SendWebRequest().ToUniTask();

            var error = GetRequestError(request);
            if (string.IsNullOrEmpty(error) is true)
            {return new Result<byte[]>(request.downloadHandler.data);}

            var errorMessage = error.Contains(SheetDownloaderConstants.Error404Indicator)
                ? SheetDownloaderConstants.TableIdWrongMessage
                : error;

            return Result<byte[]>.Invalid(errorMessage);
        }

        private async UniTask FillSheetsToDownloadAsync(CancellationToken token)
        {
            _sheetsToDownload.Clear();

            foreach (var sheet in _database.Sheets)
            {
                if (!sheet.TextAsset)
                {
                    _sheetsToDownload.Add(sheet);
                    continue;
                }

                var contentLength = await GetSheetContentLengthAsync(sheet.Id, token);

                Debug.Log($"[SheetsDownloader::FillSheetsToDownloadAsync] Checking sheet {sheet.Name} length...");

                if (contentLength > 0 && sheet.HasChanged(contentLength))
                    _sheetsToDownload.Add(sheet);
            }
        }

        private Result ProcessResolveResponse(UnityWebRequest request)
        {
            var error = ExtractInternalError(request);
            if (error != null)
                return Result.Invalid(ZString.Format(SheetDownloaderConstants.TableNotFoundOrNoPermissionFormat, error));

            var sheets = JsonConvert.DeserializeObject<Dictionary<string, long>>(request.downloadHandler.text);

            if (sheets == null)
                return Result.Invalid(SheetDownloaderConstants.FailedToParseResponseMessage);

            _database.ReplaceSheets(sheets);

            return Result.Valid();
        }

        private async UniTask<long> GetSheetContentLengthAsync(long sheetId, CancellationToken token)
        {
            var url = ZString.Format(SheetDownloaderConstants.UrlPattern, _database.TableId, sheetId);

            using var request = UnityWebRequest.Head(url);
            await request.SendWebRequest().ToUniTask(cancellationToken: token);

            if (string.IsNullOrEmpty(request.error) is false)
                return 0;

            var responseHeader = request.GetResponseHeader(SheetDownloaderConstants.ContentLengthHeader);
            return long.TryParse(responseHeader, out var length)
                ? length
                : 0;
        }

        private async UniTask<TextAsset> SaveSheetDataAsync(TSheet sheet, byte[] data, CancellationToken token)
        {
            var fileName = ZString.Concat(sheet.Name, SheetDownloaderConstants.CsvExtension);
            var path = Path.Combine(_database.GetDownloadPath(), fileName);
            await File.WriteAllBytesAsync(path, data, token);

            await UniTask.SwitchToMainThread(token);

            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        }

        private static string GetRequestError(UnityWebRequest request)
        {
            if (string.IsNullOrEmpty(request.error) is false)
                return request.error;

            return request.downloadHandler.text.Contains(SheetDownloaderConstants.GoogleSignInIndicator)
                ? SheetDownloaderConstants.AccessDeniedMessage
                : null;
        }

        private static string ExtractInternalError(UnityWebRequest request)
        {
            var matches =
                Regex.Matches(request.downloadHandler.text, SheetDownloaderConstants.ErrorMessagePattern);

            if (matches.Count == 0
                && request.downloadHandler.text.Contains(SheetDownloaderConstants.GoogleScriptErrorIndicator) is false)
                return null;

            return matches.Count > 0
                ? matches[1].Groups[SheetDownloaderConstants.MessageGroupName].Value
                    .Replace(SheetDownloaderConstants.QuotReplacement, string.Empty)
                : request.downloadHandler.text;
        }

        private void PrepareDownloadFolderIfNeeded()
        {
            var downloadPath = _database.GetDownloadPath();
            if (Directory.Exists(downloadPath) is false)
                Directory.CreateDirectory(downloadPath);
        }
    }
}