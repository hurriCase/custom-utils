using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Effects;
using CustomUtils.Runtime.UI.Theme;
using CustomUtils.Runtime.UI.Theme.StyleData;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Rendering
{
    // Packs shape, styles and effects into the vertex channels read by the procedural image shader:
    // uv0 = sprite uv + fill/outline style rows, uv1 = edge distances, uv2 = corner radii,
    // uv3 = ring width, outline width, outline outside fraction, shadow style row,
    // normal = position within the rect + aspect ratio, tangent = shadow edge shift.
    internal sealed class ProceduralVertexEncoder
    {
        private const float AntialiasingMargin = 1f;
        private const float NoRingWidth = -1f;

        private readonly StyleDataHandle _fillData = new();
        private readonly StyleDataHandle _outlineData = new();
        private readonly StyleDataHandle _shadowData = new();

        internal float CalculateMargin(ProceduralOutline outline, ProceduralShadow shadow)
        {
            var outsideWidth = outline.IsVisible ? outline.Width * outline.OutsideFraction : 0f;
            var shadowOffset = shadow.Offset;
            var shadowExtent = shadow.IsEnabled ? Mathf.Max(Mathf.Abs(shadowOffset.x), Mathf.Abs(shadowOffset.y)) : 0f;
            var extent = Mathf.Max(outsideWidth, shadowExtent);
            return extent > 0f ? extent + AntialiasingMargin : 0f;
        }

        internal void Encode(
            VertexHelper vertexHelper,
            Rect rect,
            ProceduralShape shape,
            ThemeStyle? fillStyle,
            ProceduralOutline outline,
            ProceduralShadow shadow,
            float borderWidth)
        {
            UpdateStyleRows(fillStyle, outline, shadow);

            var shadowOffset = shadow.IsEnabled ? shadow.Offset : Vector2.zero;
            var shadowShift = shape.EdgeShift(shadowOffset);
            var effectData = CalculateEffectData(outline, borderWidth);
            var radii = shape.Radii;
            var vertex = new UIVertex();

            for (var i = 0; i < vertexHelper.currentVertCount; i++)
            {
                vertexHelper.PopulateUIVertex(ref vertex, i);

                var position = (Vector2)vertex.position;
                vertex.uv0 = new Vector4(vertex.uv0.x, vertex.uv0.y, _fillData.Row, _outlineData.Row);
                vertex.uv1 = shape.EdgeDistances(position);
                vertex.uv2 = radii;
                vertex.uv3 = effectData;
                vertex.normal = CalculateRectPosition(rect, position);
                vertex.tangent = shadowShift;

                vertexHelper.SetUIVertex(vertex, i);
            }
        }

        private void UpdateStyleRows(ThemeStyle? fillStyle, ProceduralOutline outline, ProceduralShadow shadow)
        {
            var fillKey = StyleDataKey.FromStyle(fillStyle.GetValueOrDefault(), false);
            var outlineKey = StyleDataKey.FromStyle(outline.Style, true);
            var shadowKey = StyleDataKey.FromColor(shadow.Color);

            _fillData.Apply(fillKey, fillStyle.HasValue && !fillKey.HasNoEffect);
            _outlineData.Apply(outlineKey, outline.IsVisible);
            _shadowData.Apply(shadowKey, shadow.IsEnabled);
        }

        private Vector4 CalculateEffectData(ProceduralOutline outline, float borderWidth)
        {
            var ringWidth = borderWidth > 0f ? borderWidth : NoRingWidth;
            var outlineWidth = outline.IsVisible ? outline.Width : 0f;
            return new Vector4(ringWidth, outlineWidth, outline.OutsideFraction, _shadowData.Row);
        }

        // Position within the rect (unclamped, the margin lies outside it) and the rect's aspect ratio.
        private Vector3 CalculateRectPosition(Rect rect, Vector2 position)
        {
            var size = rect.size;
            var x = size.x > 0f ? (position.x - rect.xMin) / size.x : 0f;
            var y = size.y > 0f ? (position.y - rect.yMin) / size.y : 0f;
            var aspectRatio = size.y > 0f ? size.x / size.y : 0f;
            return new Vector3(x, y, aspectRatio);
        }

        internal void Clear()
        {
            _fillData.Clear();
            _outlineData.Clear();
            _shadowData.Clear();
        }
    }
}
