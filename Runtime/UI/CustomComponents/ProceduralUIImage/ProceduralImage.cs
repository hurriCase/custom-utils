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

        private const float MaxPixelSize = 2048f;

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
            var info = CalculateInfo(imageRect);

            ModifierBase.EncodeShaderData(
                imageRect,
                info.NormalizedBorderWidth,
                info.PixelSize,
                out var uv2,
                out var uv3);

            var uv1 = new Vector2(info.Width, info.Height);
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

        private ProceduralImageInfo CalculateInfo(Rect imageRect)
        {
            var minSide = Mathf.Min(imageRect.width, imageRect.height);
            var normalizedBorderWidth = BorderWidth.Value / minSide;
            var pixelSize = 1f / Mathf.Max(0.0001f, FalloffDistance.Value);

            return new ProceduralImageInfo(
                imageRect.width + FalloffDistance.Value,
                imageRect.height + FalloffDistance.Value,
                FalloffDistance.Value,
                pixelSize,
                normalizedBorderWidth);
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