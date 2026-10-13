using Newtonsoft.Json;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Clipboard
{
    internal sealed class FigmaVector2Data
    {
        [JsonProperty("x")] internal float X { get; private set; }
        [JsonProperty("y")] internal float Y { get; private set; }

        internal Vector2 Value => new(X, Y);
    }
}
