using CustomUtils.Runtime.UI.GradientHelpers.GraphicGradient.Multi;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.Base;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    [ColorModifier(ColorType.GraphicMultiGradient)]
    internal sealed class MultiGradientColorModifier : GradientModifierBase<MultiGradientEffect, Graphic> { }
}