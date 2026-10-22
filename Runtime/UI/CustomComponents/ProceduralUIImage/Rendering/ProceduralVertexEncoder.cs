using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Effects;
using CustomUtils.Runtime.UI.Theme;
using CustomUtils.Runtime.UI.Theme.StyleData;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Rendering
{
    // Packs shape, styles and effects into the vertex channels read by the procedural image shader:
    // uv0 = sprite uv + position within the rect, uv1 = edge distances, uv2 = corner radii,
    // uv3 = ring width, outline width, outline outside fraction, packed fill/outline/shadow style rows,
    // tangent = shadow offset (xy) + rect aspect ratio (w).
    // Canvas batching rotates normals and tangents with the element, so the tangent's xy holds the only direction
    // and everything positional stays in the uv channels.
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
            var aspectRatio = rect.height > 0f ? rect.width / rect.height : 0f;
            var shadowOffsetAndAspect = new Vector4(shadowOffset.x, shadowOffset.y, 0f, aspectRatio);
            var effectData = CalculateEffectData(outline, borderWidth);
            var radii = shape.Radii;
            var vertex = new UIVertex();

            for (var i = 0; i < vertexHelper.currentVertCount; i++)
            {
                vertexHelper.PopulateUIVertex(ref vertex, i);

                var position = (Vector2)vertex.position;
                var rectPosition = CalculateRectPosition(rect, position);
                vertex.uv0 = new Vector4(vertex.uv0.x, vertex.uv0.y, rectPosition.x, rectPosition.y);
                vertex.uv1 = shape.EdgeDistances(position);
                vertex.uv2 = radii;
                vertex.uv3 = effectData;
                vertex.tangent = shadowOffsetAndAspect;

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
            var packedRows = PackStyleRows();
            return new Vector4(ringWidth, outlineWidth, outline.OutsideFraction, packedRows);
        }

        // One digit per row in base StyleDataTexture.PackedRowBase, shifted by one so no row is a negative digit.
        // The sum stays below 2^24, so the float holds it exactly; the shader unpacks it with the same base.
        private float PackStyleRows()
        {
            const int rowBase = StyleDataTexture.PackedRowBase;
            var fillDigit = _fillData.Row + 1;
            var outlineDigit = _outlineData.Row + 1;
            var shadowDigit = _shadowData.Row + 1;
            return fillDigit + (outlineDigit + shadowDigit * rowBase) * rowBase;
        }

        // Position within the rect, unclamped: the margin lies outside it.
        private Vector2 CalculateRectPosition(Rect rect, Vector2 position)
        {
            var size = rect.size;
            var x = size.x > 0f ? (position.x - rect.xMin) / size.x : 0f;
            var y = size.y > 0f ? (position.y - rect.yMin) / size.y : 0f;
            return new Vector2(x, y);
        }

        internal void Clear()
        {
            _fillData.Clear();
            _outlineData.Clear();
            _shadowData.Clear();
        }
    }
}
