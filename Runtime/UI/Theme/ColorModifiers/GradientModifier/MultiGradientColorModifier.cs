using CustomUtils.Runtime.Other;
using CustomUtils.Runtime.UI.GradientHelpers.Base;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.Base;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    [ColorModifier(ColorType.GraphicMultiGradient)]
    internal sealed class MultiGradientColorModifier : ShaderGradientModifierBase<GradientDirection>
    {
        protected override Shader GradientShader => ResourceReferences.Instance.MultiGradientShader;
    }
}