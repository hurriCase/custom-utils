using System.Collections.Generic;
using CustomUtils.Editor.Scripts.FigmaBridge.Clipboard;
using CustomUtils.Editor.Scripts.FigmaBridge.Halftone;
using CustomUtils.Runtime.UI.Theme;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier.ShaderGradient;
using Newtonsoft.Json;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Colors.Data
{
    internal sealed class ColorFigmaClipboardData : FigmaClipboardData
    {
        [JsonProperty("layerName")] internal string LayerName { get; private set; }
        [JsonProperty("name")] internal string Name { get; private set; }
        [JsonProperty("colorType")] internal ColorType ColorType { get; private set; }
        [JsonProperty("gradientType")] internal GradientType GradientType { get; private set; }
        [JsonProperty("rotation")] internal float Rotation { get; private set; }
        [JsonProperty("color")] internal FigmaColorData Color { get; private set; }
        [JsonProperty("stops")] internal List<FigmaGradientStopData> Stops { get; private set; }
        [JsonProperty("halftone")] internal HalftoneFigmaClipboardData Halftone { get; private set; }
    }
}
