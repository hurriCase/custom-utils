using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage
{
    internal readonly struct ProceduralShape
    {
        internal Vector2 TopLeft { get; }
        internal Vector2 TopRight { get; }
        internal Vector2 BottomRight { get; }
        internal Vector2 BottomLeft { get; }
        internal Vector4 Radii { get; }
        internal float FalloffDistance { get; }

        internal ProceduralShape(
            Vector2 topLeft,
            Vector2 topRight,
            Vector2 bottomRight,
            Vector2 bottomLeft,
            Vector4 radii,
            float falloffDistance)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
            Radii = radii;
            FalloffDistance = falloffDistance;
        }
    }
}
