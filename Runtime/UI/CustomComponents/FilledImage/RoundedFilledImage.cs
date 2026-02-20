using CustomUtils.Runtime.Constants;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.CustomComponents.FilledImage.Modifier;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents.FilledImage
{
    [RequireComponent(typeof(CanvasRenderer))]
    [ExecuteAlways]
    [PublicAPI]
    public sealed class RoundedFilledImage : Image
    {
        [field: SerializeField, Range(0, 359)] public float CustomFillOrigin { get; set; }
        [field: SerializeField, Range(0.01f, 0.5f)] public float ThicknessRatio { get; set; } = 0.2f;
        [field: SerializeField, Range(1, 100)] public int ArcResolutionPerRadian { get; set; } = 15;

        [field: SerializeField] public bool IsRoundedCaps { get; set; }
        [field: SerializeField, Range(3, 36)] public int RoundedCapResolution { get; set; } = 15;

        [field: SerializeField] public CapGeometryType CapGeometryType { get; private set; }
        [field: SerializeField] internal CapGeometryBase CapGeometry { get; private set; }

#if UNITY_EDITOR // to prevent OnValidate from updating the color
        [SerializeField, HideInInspector] private CapGeometryType _previousCapGeometryType;
#endif

        private const float AlmostZeroFill = 0.001f;
        private const float AlmostFullFill = 0.999f;

        private readonly ArcMeshBuilder _meshBuilder = new();

        protected override void OnEnable()
        {
            base.OnEnable();

            SetAllDirty();
        }

        public void ChangeCapGeometry(CapGeometryType geometryType)
        {
            CapGeometry.AsNullable()?.Destroy();
            CapGeometry = CapGeometryFactory.CreateModifier(geometryType, gameObject);
            this.MarkAsDirty();

            SetAllDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            if (type != Type.Filled || fillMethod != FillMethod.Radial360)
            {
                base.OnPopulateMesh(vertexHelper);
                return;
            }

            var arcGeometry = CalculateGeometry();
            _meshBuilder.BuildMesh(vertexHelper, arcGeometry, color);
        }

        private ArcGeometry CalculateGeometry()
        {
            var rect = rectTransform.rect;
            var center = rect.center;
            var outerRadius = Mathf.Min(rect.width, rect.height) * 0.5f;
            var thickness = outerRadius * ThicknessRatio;
            var innerRadius = outerRadius - thickness;

            var (startRadians, endRadians) = CalculateCapAngles();

            var hasRoundedCaps = IsRoundedCaps && fillAmount is > AlmostZeroFill and < AlmostFullFill && CapGeometry;

            var capParams = new CapParameters(center, innerRadius, outerRadius, RoundedCapResolution);
            var startCap = hasRoundedCaps ? CapGeometry.CreateStartCap(capParams, startRadians) : null;
            var endCap = hasRoundedCaps ? CapGeometry.CreateEndCap(capParams, endRadians) : null;

            var arcParameters = new ArcParameters(endRadians, startRadians, center, innerRadius, outerRadius);
            return new ArcGeometry(arcParameters, ArcResolutionPerRadian, hasRoundedCaps, startCap, endCap);
        }

        private (float startRadians, float endRadians) CalculateCapAngles()
        {
            var endAngle = CustomFillOrigin + MathConstants.FullCircleDegrees * fillAmount;
            return (CustomFillOrigin.ToRadians(), endAngle.ToRadians());
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            sprite = ResourceReferences.Instance.SquareSprite;
            type = Type.Filled;
            fillMethod = FillMethod.Radial360;
            this.MarkAsDirty();
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            TryUpdateCapGeometry();

            SetAllDirty();
        }

        private void TryUpdateCapGeometry()
        {
            if (PrefabUtility.IsPartOfPrefabAsset(this))
                return;

            // We can't destroy an object during OnValidate
            EditorApplication.delayCall += () =>
            {
                if (!this || _previousCapGeometryType == CapGeometryType)
                    return;

                ChangeCapGeometry(CapGeometryType);
                _previousCapGeometryType = CapGeometryType;
                this.MarkAsDirty();

                SetAllDirty();
            };
        }
#endif
    }
}