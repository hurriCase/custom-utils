using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.UI.GradientHelpers.Base;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.Base;
using CustomUtils.Runtime.UI.Theme.Databases;
using CustomUtils.Runtime.UI.Theme.Databases.Base;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier
{
    internal abstract class GradientModifierBase<TGradientEffect, TComponent> : GenericColorModifierBase<Gradient>
        where TGradientEffect : GradientEffectBase<TComponent>, new()
        where TComponent : Component
    {
        [SerializeField] private GradientDirection _gradientDirection = GradientDirection.TopToBottom;

        [SerializeField, Self] private TComponent _component;

        protected override IThemeDatabase<Gradient> ThemeDatabase => GradientColorDatabase.Instance;

        private readonly TGradientEffect _gradientEffectBase = new();

        protected override void OnUpdateColor(Gradient gradient)
        {
            _gradientEffectBase.ApplyGradient(_component, gradient, _gradientDirection);
        }

        public override void Dispose()
        {
            _gradientEffectBase.ClearGradient(_component);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateColor(currentColorName);
        }
#endif
    }
}