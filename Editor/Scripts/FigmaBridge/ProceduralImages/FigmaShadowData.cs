using CustomUtils.Editor.Scripts.FigmaBridge.Clipboard;
using CustomUtils.Editor.Scripts.FigmaBridge.Colors.Data;
using Newtonsoft.Json;

namespace CustomUtils.Editor.Scripts.FigmaBridge.ProceduralImages
{
    internal sealed class FigmaShadowData
    {
        [JsonProperty("offset")] internal FigmaVector2Data Offset { get; private set; }
        [JsonProperty("color")] internal FigmaColorData Color { get; private set; }
    }
}
