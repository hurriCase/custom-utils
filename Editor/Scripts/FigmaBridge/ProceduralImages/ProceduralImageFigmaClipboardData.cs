using CustomUtils.Editor.Scripts.FigmaBridge.Clipboard;
using CustomUtils.Editor.Scripts.FigmaBridge.Colors.Data;
using CustomUtils.Editor.Scripts.FigmaBridge.Margins;
using Newtonsoft.Json;

namespace CustomUtils.Editor.Scripts.FigmaBridge.ProceduralImages
{
    // Every section is null when the Figma layer has nothing to copy for it.
    internal sealed class ProceduralImageFigmaClipboardData : FigmaClipboardData
    {
        [JsonProperty("rectTransform")] internal RectTransformFigmaClipboardData RectTransform { get; private set; }
        [JsonProperty("cornerOffsets")] internal CornerOffsetsFigmaClipboardData CornerOffsets { get; private set; }
        [JsonProperty("cornerRadius")] internal float? CornerRadius { get; private set; }
        [JsonProperty("fill")] internal ColorFigmaClipboardData Fill { get; private set; }
        [JsonProperty("outline")] internal FigmaOutlineData Outline { get; private set; }
        [JsonProperty("shadow")] internal FigmaShadowData Shadow { get; private set; }
    }
}
