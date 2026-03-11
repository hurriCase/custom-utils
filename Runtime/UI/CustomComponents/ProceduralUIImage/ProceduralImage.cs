using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Other;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers.Base;
using JetBrains.Annotations;
using Unity.Profiling;
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

        [SerializeField, Min(0)] private float _borderWidth;
        [SerializeField, Min(0)] private float _falloffDistance;

        internal const string BorderWidthFieldName = nameof(_borderWidth);
        internal const string FalloffDistanceFieldName = nameof(_falloffDistance);

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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static readonly ProfilerMarker _markerEncodeVertices
            = new(ProfilerCategory.Render, nameof(ProceduralImage) + "." + nameof(EncodeAllInfoIntoVertices));

        private static readonly ProfilerMarker _markerCalculateInfo
            = new(ProfilerCategory.Render, nameof(ProceduralImage) + "." + nameof(CalculateInfo));
#endif

        private ResourceReferences ResourceReferences => ResourceReferences.Instance;

        private static readonly int _rectMin = Shader.PropertyToID("_RectMin");

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

        private Vector4 FixRadius(Vector4 cornerRadius)
        {
            cornerRadius = cornerRadius.ClampToPositive();
            var scaleFactor = rectTransform.rect.CalculateScaleFactorForBounds(cornerRadius);
            return cornerRadius * scaleFactor;
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            using var encodeVerticesScope = _markerEncodeVertices.Auto();
#endif

            var info = CalculateInfo();
            var imageRect = GetPixelAdjustedRect();

            var mat = materialForRendering;
            if (mat)
                mat.SetVector(_rectMin, new Vector4(imageRect.xMin, imageRect.yMin, 0, 0));

            var uv1 = new Vector2(info.Width, info.Height);
            var uv2 = new Vector2(
                info.NormalizedRadius.x.PackAs16BitWith(info.NormalizedRadius.y),
                info.NormalizedRadius.z.PackAs16BitWith(info.NormalizedRadius.w)
            );

            var uv3 = new Vector2(info.NormalizedBorderWidth == 0 ? 1 : Mathf.Clamp01(info.NormalizedBorderWidth),
                info.PixelSize);

            var vert = new UIVertex();
            for (var i = 0; i < vertexHelper.currentVertCount; i++)
            {
                vertexHelper.PopulateUIVertex(ref vert, i);

                vert.position += (Vector3)GetCornerOffset(vert.uv0);

                vert.uv1 = uv1;
                vert.uv2 = uv2;
                vert.uv3 = uv3;

                vertexHelper.SetUIVertex(vert, i);
            }
        }

        private ProceduralImageInfo CalculateInfo()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            using var calculateInfoScope = _markerCalculateInfo.Auto();
#endif

            var imageRect = GetPixelAdjustedRect();
            var pixelSize = 1f / Mathf.Max(0.0001f, FalloffDistance);

            var radius = FixRadius(ModifierBase.CalculateRadius(imageRect));

            var minSide = Mathf.Min(imageRect.width, imageRect.height);

            var normalizedRadius = radius / minSide;
            var normalizedBorderWidth = BorderWidth / minSide;

            var info = new ProceduralImageInfo(
                imageRect.width + FalloffDistance,
                imageRect.height + FalloffDistance,
                pixelSize,
                normalizedRadius,
                normalizedBorderWidth);

            return info;
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