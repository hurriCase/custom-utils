using System.Threading;
using CustomUtils.Editor.Scripts.Extensions;
using CustomUtils.Runtime.Downloader;
using CustomUtils.Runtime.Extensions;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUtils.Editor.Scripts.SheetsDownloader
{
    [UsedImplicitly]
    public abstract class SheetsDownloaderWindowBase<TDatabase, TSheet> : EditorWindow, ISheetsDownloaderWindow
        where TDatabase : SheetsDatabase<TDatabase, TSheet>
        where TSheet : Sheet, new()
    {
        protected abstract TDatabase Database { get; }

        private SheetsDownloader<TDatabase, TSheet> _sheetsDownloader;
        private SerializedObject _serializedObject;

        private CancellationTokenSource _downloadTokenSource;

        protected abstract void CreateCustomContent();
        protected virtual void OnSheetsDownloaded() { }

        public void DownloadSheet(long sheetId)
        {
            var sheet = Database.Sheets.FirstOrDefault(sheet => sheet.Id == sheetId);
            if (sheet != null)
                ProcessDownloadSingleSheetAsync(sheet).Forget();
        }

        protected void CreateGUI()
        {
            _serializedObject = new SerializedObject(Database);
            _sheetsDownloader = new SheetsDownloader<TDatabase, TSheet>(Database);

            CreateButtons();

            _serializedObject.CreateProperty(nameof(Database.TableId), rootVisualElement);
            _serializedObject.CreateProperty(nameof(Database.Sheets), rootVisualElement);

            CreateCustomContent();
        }

        private void CreateButtons()
        {
            var buttonContainer = new VisualElement();
            buttonContainer.AddToClassList("unity-base-field");

            var downloadAllButton = new Button(() => ProcessDownloadSheetsAsync().Forget())
            {
                text = "Download All Sheets",
                style = { flexGrow = 1 }
            };

            var openGoogleButton = new Button(OpenGoogleSheet)
            {
                text = "Open Google Sheet",
                style = { flexGrow = 1 }
            };

            buttonContainer.Add(downloadAllButton);
            buttonContainer.Add(openGoogleButton);

            rootVisualElement.Add(buttonContainer);
        }

        private async UniTaskVoid ProcessDownloadSingleSheetAsync(TSheet sheet)
        {
            var token = CancellationExtensions.GetFreshToken(ref _downloadTokenSource);
            var result = await _sheetsDownloader.DownloadSingleSheetAsync(sheet, token);
            result.DisplayMessage();

            if (result.IsValid)
                OnSheetsDownloaded();
        }

        private async UniTaskVoid ProcessDownloadSheetsAsync()
        {
            if (Database.Sheets.Count == 0)
            {
                var resolveResult = await _sheetsDownloader.TryResolveGoogleSheetsAsync();
                resolveResult.DisplayMessage();
            }

            var token = CancellationExtensions.GetFreshToken(ref _downloadTokenSource);
            var downloadResult = await _sheetsDownloader.DownloadSheetsAsync(token);

            downloadResult.DisplayMessage();
            if (!downloadResult.IsValid)
                return;

            OnSheetsDownloaded();
        }

        private void OpenGoogleSheet()
        {
            Application.OpenURL(ZString.Format(SheetDownloaderConstants.TableUrlPattern, Database.TableId));
        }
    }
}