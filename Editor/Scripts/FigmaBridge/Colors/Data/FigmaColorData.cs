using Newtonsoft.Json;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Colors.Data
{
    internal class FigmaColorData
    {
        [JsonProperty("r")] internal float R { get; private set; }
        [JsonProperty("g")] internal float G { get; private set; }
        [JsonProperty("b")] internal float B { get; private set; }
        [JsonProperty("a")] internal float A { get; private set; }

        internal Color Value => new(R, G, B, A);
    }
}
