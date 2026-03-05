using CustomUtils.Runtime.Extensions;
using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers.Base
{
    public abstract class RoundedModifierBase : ModifierBase
    {
        protected abstract Vector4 CalculateRadius(Rect imageRect);

        public override void EncodeShaderData(
            Rect imageRect,
            float normalizedBorderWidth,
            float pixelSize,
            out Vector2 uv2,
            out Vector2 uv3)
        {
            var minSide = Mathf.Min(imageRect.width, imageRect.height);
            var radius = CalculateRadius(imageRect).ClampToPositive();
            var scaleFactor = imageRect.CalculateScaleFactorForBounds(radius);
            var normalizedRadius = radius * scaleFactor / minSide;

            uv2 = new Vector2(
                normalizedRadius.x.PackAs16BitWith(normalizedRadius.y),
                normalizedRadius.z.PackAs16BitWith(normalizedRadius.w)
            );
            uv3 = new Vector2(
                normalizedBorderWidth.PackAs16BitWith(0.5f),
                pixelSize
            );
        }
    }
}