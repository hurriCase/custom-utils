using Newtonsoft.Json;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Colors.Data
{
    internal sealed class FigmaGradientStopData : FigmaColorData
    {
        [JsonProperty("position")] internal float Position { get; private set; }
    }
}
