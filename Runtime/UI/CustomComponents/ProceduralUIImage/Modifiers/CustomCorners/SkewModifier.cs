using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Attributes;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers.Base;
using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers.CustomCorners
{
    [ModifierID("Skew")]
    [DisallowMultipleComponent]
    public sealed class SkewModifier : ModifierBase
    {
        [field: SerializeField] public float SkewAmount { get; private set; }

        public override void EncodeShaderData(
            Rect imageRect,
            float normalizedBorderWidth,
            float pixelSize,
            out Vector2 uv2,
            out Vector2 uv3)
        {
            var normalizedSkew = Mathf.Clamp01(SkewAmount / imageRect.width * 0.5f + 0.5f);

            uv2 = Vector2.zero;
            uv3 = new Vector2(
                normalizedBorderWidth.PackAs16BitWith(normalizedSkew),
                pixelSize
            );
        }
    }
}