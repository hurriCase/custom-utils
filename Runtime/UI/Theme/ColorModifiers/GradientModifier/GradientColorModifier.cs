using CustomUtils.Runtime.UI.GradientHelpers.GraphicGradient;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.Base;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    [ColorModifier(ColorType.GraphicGradient)]
    internal sealed class GradientColorModifier : GradientModifierBase<GraphicGradientEffect, Graphic> { }
}