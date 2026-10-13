using System;
using CustomUtils.Runtime.UI.Halftone;
using Newtonsoft.Json;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Colors.Data
{
    internal sealed class FigmaHalftoneData
    {
        [JsonProperty("scaleX")] internal float ScaleX { get; private set; }
        [JsonProperty("scaleY")] internal float ScaleY { get; private set; }
        [JsonProperty("offsetX")] internal float OffsetX { get; private set; }
        [JsonProperty("offsetY")] internal float OffsetY { get; private set; }
        [JsonProperty("patternRotation")] internal float PatternRotation { get; private set; }
        [JsonProperty("opacity")] internal float Opacity { get; private set; }
        [JsonProperty("blendMode")] internal string FigmaBlendMode { get; private set; }

        private const string BlendModeWordSeparator = "_";

        internal HalftoneProperties ToHalftoneProperties()
        {
            // Figma names blend modes like "COLOR_BURN", which match HalftoneBlendMode without underscores.
            var blendModeName = FigmaBlendMode.Replace(BlendModeWordSeparator, string.Empty);
            var blendMode = Enum.Parse<HalftoneBlendMode>(blendModeName, ignoreCase: true);
            var offset = new Vector2(OffsetX, OffsetY);
            var scale = new Vector2(ScaleX, ScaleY);
            return HalftoneProperties.Create(offset, scale, Opacity, blendMode, PatternRotation);
        }
    }
}
