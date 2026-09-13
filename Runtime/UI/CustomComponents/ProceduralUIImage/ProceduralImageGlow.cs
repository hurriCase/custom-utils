using CustomUtils.Runtime.Other;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage
{
    [ExecuteAlways]
    [AddComponentMenu("UI/Procedural Image Glow")]
    public sealed class ProceduralImageGlow : MaskableGraphic
    {
        [field: SerializeField] public ProceduralImage SourceImage { get; private set; }
        [field: SerializeField] public float GlowRadius { get; private set; }

        public override Material material
        {
            get => !m_Material ? ResourceReferences.ProceduralImageGlowMaterial : base.material;
            set => base.material = value;
        }

        private ResourceReferences ResourceReferences => ResourceReferences.Instance;

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            var shapeInfo = SourceImage.CalculateInfo();
            var halfWidth = shapeInfo.Width * 0.5f;
            var halfHeight = shapeInfo.Height * 0.5f;

            var cornerBottomLeft = new Vector2(-halfWidth, -halfHeight) + SourceImage.CornerOffsetBottomLeft;
            var cornerTopLeft = new Vector2(-halfWidth, halfHeight) + SourceImage.CornerOffsetTopLeft;
            var cornerTopRight = new Vector2(halfWidth, halfHeight) + SourceImage.CornerOffsetTopRight;
            var cornerBottomRight = new Vector2(halfWidth, -halfHeight) + SourceImage.CornerOffsetBottomRight;

            var meshPadding = GlowRadius + GlowRadius;

            var minX = Mathf.Min(Mathf.Min(cornerBottomLeft.x, cornerTopLeft.x),
                Mathf.Min(cornerTopRight.x, cornerBottomRight.x)) - meshPadding;
            var maxX = Mathf.Max(Mathf.Max(cornerBottomLeft.x, cornerTopLeft.x),
                Mathf.Max(cornerTopRight.x, cornerBottomRight.x)) + meshPadding;
            var minY = Mathf.Min(Mathf.Min(cornerBottomLeft.y, cornerTopLeft.y),
                Mathf.Min(cornerTopRight.y, cornerBottomRight.y)) - meshPadding;
            var maxY = Mathf.Max(Mathf.Max(cornerBottomLeft.y, cornerTopLeft.y),
                Mathf.Max(cornerTopRight.y, cornerBottomRight.y)) + meshPadding;

            AddGlowQuad(vertexHelper, minX, maxX, minY, maxY, cornerBottomLeft, cornerTopLeft, cornerTopRight,
                cornerBottomRight);
        }

        private void AddGlowQuad(
            VertexHelper vertexHelper,
            float minX,
            float maxX,
            float minY,
            float maxY,
            Vector2 cornerBottomLeft,
            Vector2 cornerTopLeft,
            Vector2 cornerTopRight,
            Vector2 cornerBottomRight)
        {
            var packedGlowRadius = new Vector3(GlowRadius, 0.0f, 0.0f);

            vertexHelper.AddVert(new Vector3(minX, minY), color, cornerBottomLeft, cornerTopLeft, cornerTopRight,
                cornerBottomRight, packedGlowRadius, Vector4.zero);
            vertexHelper.AddVert(new Vector3(minX, maxY), color, cornerBottomLeft, cornerTopLeft, cornerTopRight,
                cornerBottomRight, packedGlowRadius, Vector4.zero);
            vertexHelper.AddVert(new Vector3(maxX, maxY), color, cornerBottomLeft, cornerTopLeft, cornerTopRight,
                cornerBottomRight, packedGlowRadius, Vector4.zero);
            vertexHelper.AddVert(new Vector3(maxX, minY), color, cornerBottomLeft, cornerTopLeft, cornerTopRight,
                cornerBottomRight, packedGlowRadius, Vector4.zero);

            vertexHelper.AddTriangle(0, 1, 2);
            vertexHelper.AddTriangle(2, 3, 0);
        }
    }
}