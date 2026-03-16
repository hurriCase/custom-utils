using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.GradientHelpers.Base;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.GradientHelpers.GraphicGradient.Multi
{
    /// <summary>
    /// A BaseMeshEffect implementation that applies multi-stop gradient colors to UI element vertices.
    /// </summary>
    [ExecuteAlways]
    [PublicAPI]
    public sealed class VertexMultiGradientEffect : BaseMeshEffect
    {
        [SerializeField, InspectorReadOnly] private GradientDirection _direction;

        private Gradient _gradient;
        private Rect _rect;

        /// <summary>
        /// Sets the gradient parameters for this effect.
        /// </summary>
        /// <param name="gradient">The gradient containing color stops to evaluate per vertex.</param>
        /// <param name="direction">The direction in which the gradient should be applied.</param>
        /// <param name="rect">The bounding rect of the graphic, used to normalize vertex positions.</param>
        /// <remarks>
        /// This method configures the gradient but does not immediately apply it.
        /// The effect will be applied during the next mesh update cycle.
        /// </remarks>
        public void SetGradient(Gradient gradient, GradientDirection direction, Rect rect)
        {
            _gradient = gradient;
            _direction = direction;
            _rect = rect;
            this.MarkAsDirty();
        }

        /// <summary>
        /// Modifies the mesh vertices to apply the multi-stop gradient effect.
        /// </summary>
        /// <param name="vertexHelper">The VertexHelper containing the mesh vertices to modify.</param>
        /// <remarks>
        /// This method is called automatically by Unity's UI system when the mesh needs to be updated.
        /// Each vertex position is normalized within the graphic's rect along the gradient axis,
        /// then evaluated against the gradient to determine its color.
        /// </remarks>
        public override void ModifyMesh(VertexHelper vertexHelper)
        {
            if (_direction == GradientDirection.None || _gradient == null)
                return;

            var isHorizontal = _direction is GradientDirection.LeftToRight or GradientDirection.RightToLeft;
            var isReversed = _direction is GradientDirection.RightToLeft or GradientDirection.TopToBottom;
            var vertexCount = vertexHelper.currentVertCount;
            var vertex = default(UIVertex);

            for (var i = 0; i < vertexCount; i++)
            {
                vertexHelper.PopulateUIVertex(ref vertex, i);

                var t = isHorizontal
                    ? Mathf.InverseLerp(_rect.xMin, _rect.xMax, vertex.position.x)
                    : Mathf.InverseLerp(_rect.yMin, _rect.yMax, vertex.position.y);

                if (isReversed)
                    t = 1f - t;

                vertex.color = _gradient.Evaluate(t);
                vertexHelper.SetUIVertex(vertex, i);
            }
        }
    }
}