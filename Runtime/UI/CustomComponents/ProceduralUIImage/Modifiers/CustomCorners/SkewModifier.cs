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

        public override Vector4 CalculateRadius(Rect imageRect) => Vector4.zero;
        public override float CalculateSkew() => SkewAmount;
    }
}