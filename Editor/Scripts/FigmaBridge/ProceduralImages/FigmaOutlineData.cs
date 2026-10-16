using CustomUtils.Editor.Scripts.FigmaBridge.Colors.Data;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Effects;
using Newtonsoft.Json;

namespace CustomUtils.Editor.Scripts.FigmaBridge.ProceduralImages
{
    internal sealed class FigmaOutlineData
    {
        [JsonProperty("width")] internal float Width { get; private set; }
        [JsonProperty("alignment")] internal OutlineAlignment Alignment { get; private set; }
        [JsonProperty("style")] internal ColorFigmaClipboardData Style { get; private set; }
    }
}
