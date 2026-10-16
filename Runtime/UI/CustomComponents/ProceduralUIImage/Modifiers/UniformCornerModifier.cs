using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Attributes;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers.Base;
using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers
{
    [ModifierID("Uniform")]
    [DisallowMultipleComponent]
    public sealed class UniformCornerModifier : ModifierBase
    {
        [field: SerializeField] public float Radius { get; private set; }

        public void SetRadius(float radius)
        {
            Radius = Mathf.Max(0f, radius);

            if (TryGetComponent<ProceduralImage>(out var proceduralImage))
                proceduralImage.SetVerticesDirty();
        }

        public override Vector4 CalculateRadius(Rect imageRect)
        {
            var minSide = Mathf.Min(imageRect.width, imageRect.height);
            var maxAllowedRadius = minSide * 0.5f;
            var actualRadius = Mathf.Min(Radius, maxAllowedRadius);

            return new Vector4(actualRadius, actualRadius, actualRadius, actualRadius);
        }
    }
}