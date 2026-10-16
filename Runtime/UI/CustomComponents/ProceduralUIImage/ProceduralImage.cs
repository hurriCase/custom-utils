using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Other;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Corners;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Effects;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers.Base;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Rendering;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Shape;
using CustomUtils.Runtime.UI.Theme;
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
        [field: SerializeField] public bool UseCustomMaterial { get; private set; }
        [field: SerializeField] public CornerOffsets CornerOffsets { get; private set; }
        [field: SerializeField] public ProceduralOutline Outline { get; private set; }
        [field: SerializeField] public ProceduralShadow Shadow { get; private set; }

        [field: SerializeField, Min(0)] public float BorderWidth { get; private set; }
        [field: SerializeField] public ProceduralImage ShapeSource { get; private set; }

        public override Material material
        {
            get => !m_Material ? ResourceReferences.Instance.ProceduralImageMaterial : base.material;
            set => base.material = value;
        }

        internal ShapeSourceLink ShapeLink { get; } = new();

        private ModifierBase Modifier => _modifier ? _modifier : _modifier = GetComponent<ModifierBase>();

        internal const AdditionalCanvasShaderChannels RequiredShaderChannels =
            AdditionalCanvasShaderChannels.TexCoord1 |
            AdditionalCanvasShaderChannels.TexCoord2 |
            AdditionalCanvasShaderChannels.TexCoord3 |
            AdditionalCanvasShaderChannels.Normal |
            AdditionalCanvasShaderChannels.Tangent;

        // Clipped children reach this far under the source's outline, so their antialiased edge lies on solid outline.
        private const float ShapeSourceOutlineOverlap = 1f;

        private readonly ProceduralVertexEncoder _encoder = new();

        private ThemeStyle? _fillStyle;
        private ModifierBase _modifier;

        protected override void OnEnable()
        {
            base.OnEnable();

            FixTexCoordsInCanvas();

            m_OnDirtyVertsCallback += OnVerticesDirty;

            // Adding a component dirties the graphic, so the default modifier must never be added during a rebuild.
            if (!Modifier)
                AddModifier(typeof(UniformCornerModifier));

            if (!UseCustomMaterial)
                material = null;

            OnVerticesDirty();
            UpdateShapeSource();
        }

        public void SetModifierType(System.Type modifierType)
        {
            if (TryGetComponent<ModifierBase>(out var currentModifier) && currentModifier.GetType() == modifierType)
                return;

            DestroyImmediate(currentModifier);
            AddModifier(modifierType);
            SetAllDirty();
        }

        public void SetBorderWidth(float width)
        {
            BorderWidth = Mathf.Max(0f, width);
            SetVerticesDirty();
        }

        public void SetOutline(ProceduralOutline outline)
        {
            Outline = outline;
            SetVerticesDirty();
        }

        public void SetShadow(ProceduralShadow shadow)
        {
            Shadow = shadow;
            SetVerticesDirty();
        }

        // Offsets in any mode are stored in this image's current mode.
        public void SetCornerOffsets(CornerOffsets offsets)
        {
            var size = GetPixelAdjustedRect().size;
            CornerOffsets = offsets.ConvertTo(CornerOffsets.Mode, size);
            SetVerticesDirty();
        }

        public void SetCornerOffsetMode(CornerOffsetMode mode)
        {
            var size = GetPixelAdjustedRect().size;
            CornerOffsets = CornerOffsets.ConvertTo(mode, size);
            SetVerticesDirty();
        }

        public CornerOffsets GetCornerOffsetsInPixels()
        {
            var size = GetPixelAdjustedRect().size;
            return CornerOffsets.ConvertTo(CornerOffsetMode.Absolute, size);
        }

        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();

            ShapeLink.DirtyDependents();
        }

        internal void SetFillStyle(ThemeStyle style)
        {
            _fillStyle = style;
            SetVerticesDirty();
        }

        internal void ClearFillStyle()
        {
            _fillStyle = null;
            SetVerticesDirty();
        }

        internal ProceduralShape GetShape()
        {
            var rect = GetPixelAdjustedRect();
            var radii = CalculateRadii(rect);
            return new ProceduralShape(rect, CornerOffsets, radii);
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            base.OnPopulateMesh(toFill);

            var rect = GetPixelAdjustedRect();
            var margin = QuadBuilder.IsSimpleQuad(toFill) ? _encoder.CalculateMargin(Outline, Shadow) : 0f;
            var source = ShapeLink.Source;
            var shape = source ? GetSourceShape(source) : GetShape();

            QuadBuilder.Build(toFill, rect, shape, margin, source);
            _encoder.Encode(toFill, rect, shape, _fillStyle, Outline, Shadow, BorderWidth);
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();

            FixTexCoordsInCanvas();
        }

        // A canvas added at runtime (e.g. to lift an element above an overlay) starts without the extra channels.
        protected override void OnCanvasHierarchyChanged()
        {
            base.OnCanvasHierarchyChanged();

            FixTexCoordsInCanvas();
        }

        private void AddModifier(System.Type modifierType)
        {
            gameObject.AddComponent(modifierType);
            _modifier = GetComponent<ModifierBase>();
        }

        private Vector4 CalculateRadii(Rect rect)
        {
            if (!Modifier)
                return Vector4.zero;

            var cornerRadius = Modifier.CalculateRadius(rect).ClampToPositive();
            var scaleFactor = rectTransform.rect.CalculateScaleFactorForBounds(cornerRadius);
            return cornerRadius * scaleFactor;
        }

        private ProceduralShape GetSourceShape(ProceduralImage source)
        {
            var sourceOutline = source.Outline;
            var outlineInsideWidth = sourceOutline.Width * (1f - sourceOutline.OutsideFraction);
            var inset = sourceOutline.IsVisible ? Mathf.Max(outlineInsideWidth - ShapeSourceOutlineOverlap, 0f) : 0f;

            return source.GetShape()
                .Shrink(inset)
                .Transform(source.rectTransform, rectTransform);
        }

        private void UpdateShapeSource()
        {
            var source = isActiveAndEnabled && ShapeSource != this ? ShapeSource : null;
            ShapeLink.Follow(this, source);
        }

        private void OnVerticesDirty()
        {
            if (!sprite)
                sprite = ResourceReferences.Instance.EmptySprite;
        }

        private void FixTexCoordsInCanvas()
        {
            if (canvas)
                canvas.additionalShaderChannels |= RequiredShaderChannels;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            UpdateShapeSource();
        }

        protected override void Reset()
        {
            base.Reset();

            OnEnable();
        }
#endif

        protected override void OnDisable()
        {
            base.OnDisable();

            m_OnDirtyVertsCallback -= OnVerticesDirty;

            UpdateShapeSource();
            _encoder.Clear();
        }
    }
}
