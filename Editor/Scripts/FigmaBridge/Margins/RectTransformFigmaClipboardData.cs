using Newtonsoft.Json;
using CustomUtils.Editor.Scripts.FigmaBridge.Clipboard;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Margins
{
    internal sealed class RectTransformFigmaClipboardData : FigmaClipboardData
    {
        [JsonProperty("childName")] internal string ChildName { get; private set; }
        [JsonProperty("width")] internal float Width { get; private set; }
        [JsonProperty("height")] internal float Height { get; private set; }
        [JsonProperty("left")] internal float Left { get; private set; }
        [JsonProperty("top")] internal float Top { get; private set; }
        [JsonProperty("right")] internal float Right { get; private set; }
        [JsonProperty("bottom")] internal float Bottom { get; private set; }
        [JsonProperty("rotation")] internal float Rotation { get; private set; }
    }
}
