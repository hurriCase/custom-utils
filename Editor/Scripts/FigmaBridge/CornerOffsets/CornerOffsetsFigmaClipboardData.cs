using Newtonsoft.Json;
using CustomUtils.Editor.Scripts.FigmaBridge.Clipboard;

namespace CustomUtils.Editor.Scripts.FigmaBridge.CornerOffsets
{
    internal sealed class CornerOffsetsFigmaClipboardData : FigmaClipboardData
    {
        [JsonProperty("topLeft")] internal FigmaVector2Data TopLeft { get; private set; }
        [JsonProperty("topRight")] internal FigmaVector2Data TopRight { get; private set; }
        [JsonProperty("bottomRight")] internal FigmaVector2Data BottomRight { get; private set; }
        [JsonProperty("bottomLeft")] internal FigmaVector2Data BottomLeft { get; private set; }
    }
}
