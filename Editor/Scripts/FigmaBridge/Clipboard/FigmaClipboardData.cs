using Newtonsoft.Json;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Clipboard
{
    internal abstract class FigmaClipboardData
    {
        [JsonProperty("source")] internal string Source { get; private set; }
    }
}
