using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.GradientHelpers.Base;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.GradientHelpers.GraphicGradient
{
    /// <summary>
    /// A BaseMeshEffect implementation that applies gradient colors to UI element vertices.
    /// </summary>
    /// <remarks>
    /// This component modifies mesh vertices to create gradient effects on UI graphics.
    /// It works by manipulating the color of individual vertices in quad formations.
    /// </remarks>
    [ExecuteAlways]
    [PublicAPI]
    public sealed class VertexGradientEffect : BaseMeshEffect
    {
        [SerializeField, InspectorReadOnly] private Color _startColor;
        [SerializeField, InspectorReadOnly] private Color _endColor;
        [SerializeField, InspectorReadOnly] private GradientDirection _direction;

        private const int VerticesPerQuad = 4;
        private const int BottomLeftIndex = 0;
        private const int TopLeftIndex = 1;
        private const int TopRightIndex = 2;
        private const int BottomRightIndex = 3;

        private readonly UIVertex[] _quadVertices = new UIVertex[VerticesPerQuad];

        /// <summary>
        /// Sets the gradient parameters for this effect.
        /// </summary>
        /// <param name="startColor">The starting color of the gradient.</param>
        /// <param name="endColor">The ending color of the gradient.</param>
        /// <param name="direction">The direction in which the gradient should be applied.</param>
        /// <remarks>
        /// This method configures the gradient but does not immediately apply it.
        /// The effect will be applied during the next mesh update cycle.
        /// </remarks>
        public void SetGradient(Color startColor, Color endColor, GradientDirection direction)
        {
            _startColor = startColor;
            _endColor = endColor;
            _direction = direction;
            this.MarkAsDirty();
        }

        /// <summary>
        /// Modifies the mesh vertices to apply the gradient effect.
        /// </summary>
        /// <param name="vertexHelper">The VertexHelper containing the mesh vertices to modify.</param>
        /// <remarks>
        /// This method is called automatically by Unity's UI system when the mesh needs to be updated.
        /// It processes vertices in groups of four (quads) and applies gradient colors based on the configured direction.
        /// </remarks>
        public override void ModifyMesh(VertexHelper vertexHelper)
        {
            if (_direction == GradientDirection.None)
                return;

            var vertexCount = vertexHelper.currentVertCount;

            for (var i = 0; i < vertexCount; i += VerticesPerQuad)
            {
                if (i + BottomRightIndex >= vertexCount)
                    break;

                for (var vertexIndex = 0; vertexIndex < VerticesPerQuad; vertexIndex++)
                    vertexHelper.PopulateUIVertex(ref _quadVertices[vertexIndex], i + vertexIndex);

                ApplyColor();

                for (var vertexIndex = 0; vertexIndex < VerticesPerQuad; vertexIndex++)
                    vertexHelper.SetUIVertex(_quadVertices[vertexIndex], i + vertexIndex);
            }
        }

        private void ApplyColor()
        {
            var isHorizontal = _direction is GradientDirection.LeftToRight or GradientDirection.RightToLeft;
            var isReversed = _direction is GradientDirection.RightToLeft or GradientDirection.TopToBottom;

            var (firstColor, secondColor) = isReversed ? (_endColor, _startColor) : (_startColor, _endColor);

            _quadVertices[BottomLeftIndex].color = firstColor;
            _quadVertices[TopLeftIndex].color = isHorizontal ? firstColor : secondColor;
            _quadVertices[TopRightIndex].color = secondColor;
            _quadVertices[BottomRightIndex].color = isHorizontal ? secondColor : firstColor;
        }
    }
}