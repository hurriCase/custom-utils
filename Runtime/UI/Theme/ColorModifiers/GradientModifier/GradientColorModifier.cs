using CustomUtils.Runtime.UI.Theme.ColorModifiers.Base;
using CustomUtils.Runtime.UI.Theme.Databases;
using CustomUtils.Runtime.UI.Theme.Databases.Base;
using CustomUtils.Runtime.UI.Theme.Materials;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using System;
using R3;
#endif

namespace CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    [ColorModifier(ColorType.Gradient)]
    internal sealed class GradientColorModifier : GenericColorModifierBase<Gradient>
    {
        protected override IThemeDatabase<Gradient> ThemeDatabase => ThemeDatabaseConfig.Instance.Gradient;

#if UNITY_EDITOR
        private IDisposable _atlasSubscription;
#endif

        protected override void Awake()
        {
            base.Awake();

            UpdateColor(currentColorName, true);
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            _atlasSubscription = ThemeDatabaseConfig.Instance.Gradient.OnAtlasChanged
                .Subscribe(this, static (_, self) => self.Refresh());
#endif

            if (Graphic)
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
            Graphic.SetAllDirty();
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
            _atlasSubscription.Dispose();
#endif
            Refresh();
        }
    }
}