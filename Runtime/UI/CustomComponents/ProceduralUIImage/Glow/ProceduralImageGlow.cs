using CustomUtils.Runtime.Other;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Glow
{
    [PublicAPI]
    [ExecuteAlways]
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/Procedural Image Glow")]
    public class ProceduralImageGlow : MaskableGraphic
    {
        [field: SerializeField] public ProceduralImage Source { get; set; }
        [field: SerializeField, Min(0)] public float Blur { get; set; } = 50f;
        [field: SerializeField] public float Spread { get; set; }
        [field: SerializeField] public bool ShowBehindTransparentAreas { get; set; }

        private const float BlurToSigma = 0.5f;
        private const float CoverageSigmas = 4f;
        private const float MinSigma = 0.001f;
        private const float ExtraMargin = 2f;

        private ProceduralImage _subscribedSource;

        public override Material material
        {
            get => !m_Material ? ResourceReferences.Instance.ProceduralImageGlowMaterial : base.material;
            set => base.material = value;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            RefreshSourceSubscription();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            UnsubscribeFromSource();
        }

        private void RefreshSourceSubscription()
        {
            if (ReferenceEquals(_subscribedSource, Source))
                return;

            UnsubscribeFromSource();

            if (!Source || !isActiveAndEnabled)
                return;

            Source.RegisterDirtyVerticesCallback(SetVerticesDirty);
            _subscribedSource = Source;
        }

        private void UnsubscribeFromSource()
        {
            if (_subscribedSource)
                _subscribedSource.UnregisterDirtyVerticesCallback(SetVerticesDirty);

            _subscribedSource = null;
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            if (!Source)
                return;

            var shape = Source.GetShape();
            var center = GetPixelAdjustedRect().center;

            var topLeft = center + shape.TopLeft;
            var topRight = center + shape.TopRight;
            var bottomRight = center + shape.BottomRight;
            var bottomLeft = center + shape.BottomLeft;

            var sigma = Mathf.Max(MinSigma, Blur * BlurToSigma);
            var margin = Mathf.Ceil(sigma * CoverageSigmas + Mathf.Max(0, Spread) + ExtraMargin);

            var min = Vector2.Min(Vector2.Min(topLeft, topRight), Vector2.Min(bottomRight, bottomLeft)) -
                      Vector2.one * margin;
            var max = Vector2.Max(Vector2.Max(topLeft, topRight), Vector2.Max(bottomRight, bottomLeft)) +
                      Vector2.one * margin;

            var maskFalloff = ShowBehindTransparentAreas ? -1f : shape.FalloffDistance;
            var uv0 = new Vector4(sigma, Spread, maskFalloff, 0f);

            var vertex = new UIVertex
            {
                color = color,
                uv0 = uv0,
                uv3 = shape.Radii,
                normal = Vector3.back,
                tangent = new Vector4(1f, 0f, 0f, -1f)
            };

            AddVertex(vertexHelper, ref vertex, new Vector2(min.x, min.y), topLeft, topRight, bottomRight, bottomLeft);
            AddVertex(vertexHelper, ref vertex, new Vector2(min.x, max.y), topLeft, topRight, bottomRight, bottomLeft);
            AddVertex(vertexHelper, ref vertex, new Vector2(max.x, max.y), topLeft, topRight, bottomRight, bottomLeft);
            AddVertex(vertexHelper, ref vertex, new Vector2(max.x, min.y), topLeft, topRight, bottomRight, bottomLeft);

            vertexHelper.AddTriangle(0, 1, 2);
            vertexHelper.AddTriangle(2, 3, 0);
        }

        private static void AddVertex(
            VertexHelper vertexHelper,
            ref UIVertex vertex,
            Vector2 position,
            Vector2 topLeft,
            Vector2 topRight,
            Vector2 bottomRight,
            Vector2 bottomLeft)
        {
            var toTopLeft = topLeft - position;
            var toTopRight = topRight - position;
            var toBottomRight = bottomRight - position;
            var toBottomLeft = bottomLeft - position;

            vertex.position = position;
            vertex.uv1 = new Vector4(toTopLeft.x, toTopLeft.y, toTopRight.x, toTopRight.y);
            vertex.uv2 = new Vector4(toBottomRight.x, toBottomRight.y, toBottomLeft.x, toBottomLeft.y);

            vertexHelper.AddVert(vertex);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (isActiveAndEnabled)
                RefreshSourceSubscription();
        }

        protected override void Reset()
        {
            base.Reset();

            raycastTarget = false;
        }
#endif
    }
}
