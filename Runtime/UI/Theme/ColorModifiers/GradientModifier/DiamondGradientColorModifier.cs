using CustomUtils.Runtime.Other;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.Base;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    [ColorModifier(ColorType.DiamondGradient)]
    internal sealed class DiamondGradientColorModifier : ShaderGradientModifierBase<DiamondGradientDirection>
    {
        protected override Shader GradientShader => ResourceReferences.Instance.DiamondGradientShader;
    }
}