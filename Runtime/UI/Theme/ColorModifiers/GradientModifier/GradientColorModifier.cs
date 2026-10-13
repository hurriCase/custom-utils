using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Attributes.ShowIf;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.Base;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier.ShaderGradient;
using CustomUtils.Runtime.UI.Theme.Databases;
using CustomUtils.Runtime.UI.Theme.Databases.Base;
using CustomUtils.Runtime.UI.Theme.Materials;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    [ColorModifier(ColorType.Gradient)]
    internal sealed class GradientColorModifier : GenericColorModifierBase<Gradient>
    {
        [field: SerializeField] internal GradientType GradientType { get; private set; }

        [field: SerializeField, ShowIf(nameof(GradientType), GradientType.Linear, GradientType.Angular)]
        internal float GradientRotation { get; private set; }

        [field: SerializeField, ShowIf(nameof(GradientType), GradientType.Diamond)]
        internal DiamondGradientDirection DiamondDirection { get; private set; }

        [SerializeField, Self] private Graphic _graphic;

        protected override IThemeDatabase<Gradient> ThemeDatabase => ThemeDatabaseConfig.Instance.Gradient;

        protected override void Awake()
        {
            base.Awake();

            UpdateColor(currentColorName, true);
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            ThemeDatabaseConfig.Instance.Gradient.OnAtlasChanged += Refresh;
#endif
            Refresh();
        }

        internal override void ApplyGradientSettings(GradientType gradientType, float gradientRotation)
        {
            GradientType = gradientType;
            GradientRotation = gradientRotation;
            Refresh();
        }

        internal override Material ModifyMaterial(Material baseMaterial)
            => isActiveAndEnabled ? SharedGradientMaterials.Get(baseMaterial) : baseMaterial;

        internal override GradientVertexData GetGradientVertexData()
        {
            var atlasRow = ThemeDatabaseConfig.Instance.Gradient.GetAtlasRow(currentColorName);
            return new GradientVertexData(atlasRow, GradientType, GradientRotation, DiamondDirection);
        }

        protected override void OnUpdateColor(Gradient gradient)
        {
            Refresh();
        }

        private void Refresh()
        {
            _graphic = _graphic.AsNullable() ?? GetComponent<Graphic>();
            _graphic.SetAllDirty();
        }

        public override void Dispose()
        {
            Refresh();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateColor(currentColorName, true);
        }
#endif

        private void OnDisable()
        {
#if UNITY_EDITOR
            ThemeDatabaseConfig.Instance.Gradient.OnAtlasChanged -= Refresh;
#endif
            Refresh();
        }
    }
}
