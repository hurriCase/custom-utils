using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.Base;
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

        internal override Material ModifyMaterial(Material baseMaterial)
            => isActiveAndEnabled ? SharedGradientMaterials.Get(baseMaterial) : baseMaterial;

        internal override GradientVertexData GetGradientVertexData(ThemeStyle style)
        {
            var atlasRow = ThemeDatabaseConfig.Instance.Gradient.GetAtlasRow(currentColorName);
            return new GradientVertexData(atlasRow, style.GradientType, style.GradientRotation, style.DiamondDirection);
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
