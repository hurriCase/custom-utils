using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.Theme.Materials
{
    // The canvas batcher moves vertices into canvas space, so element-relative data has to travel per vertex.
    internal sealed class ThemeVertexWriter
    {
        internal const int ThemeDataChannel = 2;
        internal const int GradientSettingsChannel = 3;

        private const AdditionalCanvasShaderChannels ThemeShaderChannels =
            AdditionalCanvasShaderChannels.TexCoord2 | AdditionalCanvasShaderChannels.TexCoord3;

        internal void Write(VertexHelper vertexHelper, Graphic graphic, GradientVertexData gradientVertexData)
        {
            var rect = graphic.rectTransform.rect;
            var gradientSettings = GetGradientSettings(gradientVertexData);
            var vertex = new UIVertex();

            for (var i = 0; i < vertexHelper.currentVertCount; i++)
            {
                vertexHelper.PopulateUIVertex(ref vertex, i);
                vertex.uv2 = GetThemeData(vertex.position, rect, gradientVertexData);
                vertex.uv3 = gradientSettings;
                vertexHelper.SetUIVertex(vertex, i);
            }

            EnableShaderChannels(graphic);
        }

        internal Vector4 GetThemeData(Vector3 position, Rect rect, GradientVertexData gradientVertexData)
        {
            // Not Rect.PointToNormalized: it clamps, which flattens glyphs that overflow the rect.
            var normalizedPosition = ((Vector2)position - rect.min) / rect.size;
            return new Vector4(
                normalizedPosition.x,
                normalizedPosition.y,
                gradientVertexData.AtlasRow,
                (float)gradientVertexData.GradientType);
        }

        internal Vector4 GetGradientSettings(GradientVertexData gradientVertexData)
            => new(gradientVertexData.Rotation, (float)gradientVertexData.DiamondDirection, 0f, 0f);

        internal void EnableShaderChannels(Graphic graphic)
        {
            graphic.canvas.additionalShaderChannels |= ThemeShaderChannels;
        }
    }
}
