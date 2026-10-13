using CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier.ShaderGradient;

namespace CustomUtils.Runtime.UI.Theme.Materials
{
    internal readonly struct GradientVertexData
    {
        internal int AtlasRow { get; }
        internal GradientType GradientType { get; }
        internal float Rotation { get; }
        internal DiamondGradientDirection DiamondDirection { get; }

        internal GradientVertexData(
            int atlasRow,
            GradientType gradientType,
            float rotation,
            DiamondGradientDirection diamondDirection)
        {
            AtlasRow = atlasRow;
            GradientType = gradientType;
            Rotation = rotation;
            DiamondDirection = diamondDirection;
        }
    }
}
