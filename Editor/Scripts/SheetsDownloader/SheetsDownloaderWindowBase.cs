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
    [PublicAPI]
    public abstract class SheetsDownloaderWindowBase<TDatabase, TSheet> : EditorWindow, ISheetsDownloaderWindow
        where TDatabase : SheetsDatabase<TDatabase, TSheet>
        where TSheet : Sheet, new()
    {
        protected abstract TDatabase Database { get; }

        private SheetsDownloader<TDatabase, TSheet> _sheetsDownloader;
        private SerializedObject _serializedObject;

        private CancellationTokenSource _downloadTokenSource;

        protected abstract void CreateCustomContent(VisualElement parent);
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

            var scrollView = new ScrollView();
            rootVisualElement.Add(scrollView);

            var scrollContent = scrollView.contentContainer;

            CreateButtons(scrollContent);

            _serializedObject.CreateProperty(nameof(Database.TableId), scrollContent);
            _serializedObject.CreateProperty(nameof(Database.Sheets), scrollContent);

            CreateCustomContent(scrollContent);
        }

        private void CreateButtons(VisualElement parent)
        {
            var buttonContainer = new VisualElement();
            buttonContainer.AddToClassList("unity-base-field");

            var downloadAllButton = new Button
            {
                text = "Download All Sheets",
                style = { flexGrow = 1 },
                clickable = new Clickable(() => ProcessDownloadSheetsAsync().Forget())
            };

            var openGoogleButton = new Button
            {
                text = "Open Google Sheet",
                style = { flexGrow = 1 },
                clickable = new Clickable(OpenGoogleSheet)
            };

            buttonContainer.Add(downloadAllButton);
            buttonContainer.Add(openGoogleButton);

            parent.Add(buttonContainer);
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
            var url = ZString.Format(SheetDownloaderConstants.TableUrlPattern, Database.TableId);
            Application.OpenURL(url);
        }
    }
}