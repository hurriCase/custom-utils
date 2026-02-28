using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Extensions.Observables;
using CustomUtils.Runtime.Other;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers.Base;
using JetBrains.Annotations;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage
{
    [PublicAPI]
    [ExecuteAlways]
    [AddComponentMenu("UI/Procedural Image")]
    public class ProceduralImage : Image
    {
        [field: SerializeField] public SerializableReactiveProperty<float> BorderWidth { get; set; } = new();

        [field: SerializeField] public SerializableReactiveProperty<float> FalloffDistance { get; set; } = new();

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

        private ModifierBase AddNewModifier(System.Type modifierType)
        {
            gameObject.AddComponent(modifierType);
            _modifierBase = GetComponent<ModifierBase>();
            return _modifierBase;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            BorderWidth.SubscribeUntilDestroy(this, static self => self.SetVerticesDirty());
            FalloffDistance.SubscribeUntilDestroy(this, static self => self.SetVerticesDirty());

            Initialize();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            m_OnDirtyVertsCallback -= OnVerticesDirty;
        }

        private void Initialize()
        {
            FixTexCoordsInCanvas();

            m_OnDirtyVertsCallback += OnVerticesDirty;
            preserveAspect = false;
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

#if UNITY_EDITOR
        public void Update()
        {
            if (!Application.isPlaying)
                UpdateGeometry();
        }
#endif

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
            var info = CalculateInfo();
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

                vert.position += ((Vector3)vert.uv0 - new Vector3(0.5f, 0.5f)) * info.FallOffDistance;

                vert.uv1 = uv1;
                vert.uv2 = uv2;
                vert.uv3 = uv3;

                vertexHelper.SetUIVertex(vert, i);
            }
        }

        private ProceduralImageInfo CalculateInfo()
        {
            var imageRect = GetPixelAdjustedRect();
            var pixelSize = 1f / Mathf.Max(0, FalloffDistance.Value);

            var radius = FixRadius(ModifierBase.CalculateRadius(imageRect));

            var minSide = Mathf.Min(imageRect.width, imageRect.height);

            var normalizedRadius = radius / minSide;
            var normalizedBorderWidth = BorderWidth.Value / minSide;

            var info = new ProceduralImageInfo(
                imageRect.width + FalloffDistance.Value,
                imageRect.height + FalloffDistance.Value,
                FalloffDistance.Value,
                pixelSize,
                normalizedRadius,
                normalizedBorderWidth);

            return info;
        }

        public override Material material
        {
            get => !m_Material ? ResourceReferences.ProceduralImageMaterial : base.material;
            set => base.material = value;
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            OnEnable();
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            FalloffDistance.Value = Mathf.Max(0, FalloffDistance.Value);
            BorderWidth.Value = Mathf.Max(0, BorderWidth.Value);
        }
#endif
    }
}