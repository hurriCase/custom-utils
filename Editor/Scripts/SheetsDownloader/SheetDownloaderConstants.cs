namespace CustomUtils.Editor.Scripts.SheetsDownloader
{
    internal static class SheetDownloaderConstants
    {
        internal const string TableUrlPattern = "https://docs.google.com/spreadsheets/d/";

        internal const string UrlPattern = "https://docs.google.com/spreadsheets/d/{0}/export?format=csv&gid={1}";
        internal const string SheetResolverUrl =
            "https://script.google.com/macros/s/" +
            "AKfycbycW2dsGZhc2xJh2Fs8yu9KUEqdM-ssOiK1AlES3crLqQa1lkDrI4mZgP7sJhmFlGAD/exec";

        internal const string GoogleSignInIndicator = "signin/identifier";
        internal const string ContentLengthHeader = "Content-Length";
        internal const string CsvExtension = ".csv";
        internal const string GoogleScriptErrorIndicator = "Google Script ERROR:";
        internal const string ErrorMessagePattern = @">(?<Message>.+?)<\/div>";
        internal const string QuotReplacement = "quot;";
        internal const string MessageGroupName = "Message";
        internal const string RequestUrlFormat = "{0}?tableUrl={1}";

        internal const string AccessDeniedMessage = "It seems that access to this document is denied.";
        internal const string TableIdWrongMessage = "Table Id is wrong!";
        internal const string NetworkErrorFormat = "Network error: ";
        internal const string AllSheetsUpToDateMessage = "All sheets are up to date!";
        internal const string ChangedSheetsDownloadedFormat = "{0} changed sheets downloaded!";
        internal const string SheetIsNullMessage = "Sheet is null";
        internal const string Error404Indicator = "404";
        internal const string SheetDownloadedSuccessFormat = "Sheet '{0}' downloaded successfully!";
        internal const string SheetDownloadFailedFormat = "Failed to download sheet '{0}': {1}";
        internal const string FailedToParseResponseMessage = "Failed to parse sheets response";
        internal const string TableNotFoundOrNoPermissionFormat =
            "Table not found or public read permission not set: ";
    }
}