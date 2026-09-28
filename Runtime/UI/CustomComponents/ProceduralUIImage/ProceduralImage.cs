using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Other;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers.Base;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage
{
    [PublicAPI]
    [ExecuteAlways]
    [AddComponentMenu("UI/Procedural Image")]
    public class ProceduralImage : Image
    {
        [field: SerializeField] public bool UseCustomMaterial { get; set; }
        [field: SerializeField] public Vector2 CornerOffsetTopLeft { get; set; }
        [field: SerializeField] public Vector2 CornerOffsetTopRight { get; set; }
        [field: SerializeField] public Vector2 CornerOffsetBottomRight { get; set; }
        [field: SerializeField] public Vector2 CornerOffsetBottomLeft { get; set; }

        [SerializeField] private CornerOffsetMode _cornerOffsetMode;
        [SerializeField, Min(0)] private float _borderWidth;
        [SerializeField, Min(0)] private float _falloffDistance;

        internal const string CornerOffsetModeFieldName = nameof(_cornerOffsetMode);
        internal const string BorderWidthFieldName = nameof(_borderWidth);
        internal const string FalloffDistanceFieldName = nameof(_falloffDistance);

        private const float MinEdgeLength = 0.001f;

        public CornerOffsetMode CornerOffsetMode
        {
            get => _cornerOffsetMode;
            set
            {
                if (_cornerOffsetMode == value)
                    return;

                _cornerOffsetMode = value;
                SetVerticesDirty();
            }
        }

        public float BorderWidth
        {
            get => _borderWidth;
            set
            {
                if (Mathf.Approximately(_borderWidth, value))
                    return;

                _borderWidth = Mathf.Max(0, value);
                SetVerticesDirty();
            }
        }

        public float FalloffDistance
        {
            get => _falloffDistance;
            set
            {
                if (Mathf.Approximately(_falloffDistance, value))
                    return;

                _falloffDistance = Mathf.Max(0, value);
                SetVerticesDirty();
            }
        }

        public override Material material
        {
            get => !m_Material ? ResourceReferences.ProceduralImageMaterial : base.material;
            set => base.material = value;
        }

        private ResourceReferences ResourceReferences => ResourceReferences.Instance;

        private ModifierBase _modifierBase;

        private ModifierBase ModifierBase
        {
            get
            {
                if (!_modifierBase)
                    _modifierBase = TryGetComponent<ModifierBase>(out var existingModifier)
                        ? existingModifier
                        : AddNewModifier(typeof(UniformCornerModifier));

                return _modifierBase;
            }
        }

        public bool SetModifierType(System.Type modifierType)
        {
            if (TryGetComponent<ModifierBase>(out var currentModifier)
                && currentModifier.GetType() == modifierType)
                return true;

            DestroyImmediate(currentModifier);
            _modifierBase = null;

            AddNewModifier(modifierType);

            SetAllDirty();

            return true;
        }

        public void SetCornerOffsetMode(CornerOffsetMode mode, bool preserveShape = true)
        {
            if (_cornerOffsetMode == mode)
                return;

            if (preserveShape)
            {
                var size = GetPixelAdjustedRect().size;
                CornerOffsetTopLeft = ConvertOffset(ResolveOffset(CornerOffsetTopLeft, size), size, mode);
                CornerOffsetTopRight = ConvertOffset(ResolveOffset(CornerOffsetTopRight, size), size, mode);
                CornerOffsetBottomRight = ConvertOffset(ResolveOffset(CornerOffsetBottomRight, size), size, mode);
                CornerOffsetBottomLeft = ConvertOffset(ResolveOffset(CornerOffsetBottomLeft, size), size, mode);
            }

            CornerOffsetMode = mode;
        }

        public Vector2 StoredOffsetToPixels(Vector2 storedOffset)
        {
            var pixelAdjustedRect = GetPixelAdjustedRect();
            return ResolveOffset(storedOffset, pixelAdjustedRect.size);
        }

        public Vector2 PixelsToStoredOffset(Vector2 pixelOffset)
        {
            var pixelAdjustedRect = GetPixelAdjustedRect();
            return ConvertOffset(pixelOffset, pixelAdjustedRect.size, _cornerOffsetMode);
        }

        private Vector2 ResolveOffset(Vector2 storedOffset, Vector2 size)
            => _cornerOffsetMode == CornerOffsetMode.Relative ? Vector2.Scale(storedOffset, size) : storedOffset;

        private static Vector2 ConvertOffset(Vector2 pixelOffset, Vector2 size, CornerOffsetMode targetMode)
            => targetMode == CornerOffsetMode.Relative
                ? new Vector2(SafeDivide(pixelOffset.x, size.x), SafeDivide(pixelOffset.y, size.y))
                : pixelOffset;

        private static float SafeDivide(float value, float divisor)
            => Mathf.Approximately(divisor, 0f) ? 0f : value / divisor;

        private ModifierBase AddNewModifier(System.Type modifierType)
        {
            gameObject.AddComponent(modifierType);
            _modifierBase = GetComponent<ModifierBase>();
            return _modifierBase;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            FixTexCoordsInCanvas();

            m_OnDirtyVertsCallback += OnVerticesDirty;
            preserveAspect = false;

            if (!UseCustomMaterial)
                material = null;

            if (!sprite)
                sprite = ResourceReferences.EmptySprite;
        }

        private void OnVerticesDirty()
        {
            if (!sprite)
                sprite = ResourceReferences.EmptySprite;
        }

        private void FixTexCoordsInCanvas()
        {
            if (canvas)
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 |
                                                   AdditionalCanvasShaderChannels.TexCoord2 |
                                                   AdditionalCanvasShaderChannels.TexCoord3;
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            base.OnPopulateMesh(toFill);

            EncodeAllInfoIntoVertices(toFill);
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();

            FixTexCoordsInCanvas();
        }

        private void EncodeAllInfoIntoVertices(VertexHelper vertexHelper)
        {
            var imageRect = GetPixelAdjustedRect();
            var size = imageRect.size;
            var shape = GetShape();

            var topLeft = imageRect.center + shape.TopLeft;
            var topRight = imageRect.center + shape.TopRight;
            var bottomRight = imageRect.center + shape.BottomRight;
            var bottomLeft = imageRect.center + shape.BottomLeft;

            var orientation = CalculateOrientation(topLeft, topRight, bottomRight, bottomLeft);
            var topNormal = CalculateEdgeNormal(topLeft, topRight, orientation);
            var rightNormal = CalculateEdgeNormal(topRight, bottomRight, orientation);
            var bottomNormal = CalculateEdgeNormal(bottomRight, bottomLeft, orientation);
            var leftNormal = CalculateEdgeNormal(bottomLeft, topLeft, orientation);

            var lineWeight = BorderWidth > 0f ? BorderWidth : -1f;
            var pixelScale = 1f / Mathf.Max(0.0001f, FalloffDistance);

            var vert = new UIVertex();
            for (var i = 0; i < vertexHelper.currentVertCount; i++)
            {
                vertexHelper.PopulateUIVertex(ref vert, i);

                vert.position += (Vector3)ResolveOffset(GetCornerOffset(vert.uv0), size);

                var position = (Vector2)vert.position;

                vert.uv0 = new Vector4(vert.uv0.x, vert.uv0.y, lineWeight, pixelScale);
                vert.uv1 = new Vector4(
                    Vector2.Dot(position - topLeft, topNormal),
                    Vector2.Dot(position - topRight, rightNormal),
                    Vector2.Dot(position - bottomRight, bottomNormal),
                    Vector2.Dot(position - bottomLeft, leftNormal));
                vert.uv2 = shape.Radii;

                vertexHelper.SetUIVertex(vert, i);
            }
        }

        private static float CalculateOrientation(
            Vector2 topLeft,
            Vector2 topRight,
            Vector2 bottomRight,
            Vector2 bottomLeft)
        {
            var doubleArea = Cross(topLeft, topRight) + Cross(topRight, bottomRight) +
                             Cross(bottomRight, bottomLeft) + Cross(bottomLeft, topLeft);
            return doubleArea < 0f ? 1f : -1f;
        }

        private static Vector2 CalculateEdgeNormal(Vector2 start, Vector2 end, float orientation)
        {
            var edge = end - start;
            var direction = edge / Mathf.Max(edge.magnitude, MinEdgeLength);
            return new Vector2(-direction.y, direction.x) * orientation;
        }

        private static float Cross(Vector2 left, Vector2 right) => left.x * right.y - left.y * right.x;

        internal ProceduralShape GetShape()
        {
            var imageRect = GetPixelAdjustedRect();
            var size = imageRect.size;
            var halfSize = size * 0.5f;

            return new ProceduralShape(
                new Vector2(-halfSize.x, halfSize.y) + ResolveOffset(CornerOffsetTopLeft, size),
                new Vector2(halfSize.x, halfSize.y) + ResolveOffset(CornerOffsetTopRight, size),
                new Vector2(halfSize.x, -halfSize.y) + ResolveOffset(CornerOffsetBottomRight, size),
                new Vector2(-halfSize.x, -halfSize.y) + ResolveOffset(CornerOffsetBottomLeft, size),
                CalculateRadius(imageRect),
                FalloffDistance);
        }

        private Vector4 CalculateRadius(Rect imageRect)
        {
            var cornerRadius = ModifierBase.CalculateRadius(imageRect).ClampToPositive();
            var scaleFactor = rectTransform.rect.CalculateScaleFactorForBounds(cornerRadius);
            return cornerRadius * scaleFactor;
        }

        private Vector2 GetCornerOffset(Vector2 uv) => (uv.y > 0.9f, uv.x > 0.9f) switch
        {
            (true, false) => CornerOffsetTopLeft,
            (true, true) => CornerOffsetTopRight,
            (false, true) => CornerOffsetBottomRight,
            (false, false) => CornerOffsetBottomLeft,
        };

        protected override void OnDisable()
        {
            base.OnDisable();

            m_OnDirtyVertsCallback -= OnVerticesDirty;
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            OnEnable();
        }
#endif
    }
}