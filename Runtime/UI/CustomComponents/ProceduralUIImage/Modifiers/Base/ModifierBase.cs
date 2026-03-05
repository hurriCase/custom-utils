using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers.Base
{
    [PublicAPI]
    public abstract class ModifierBase : MonoBehaviour
    {
        public abstract void EncodeShaderData(
            Rect imageRect,
            float normalizedBorderWidth,
            float normalizedPixelSize,
            out Vector2 uv2,
            out Vector2 uv3);
    }
}