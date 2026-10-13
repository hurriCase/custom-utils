using System;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier.ShaderGradient;
using CustomUtils.Runtime.UI.Theme.Materials;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Theme.ColorModifiers.Base
{
    internal abstract class ColorModifierBase : MonoBehaviour, IDisposable
    {
        internal abstract void UpdateColor(string guid, bool isForce = false);

        internal virtual Material ModifyMaterial(Material baseMaterial) => baseMaterial;

        internal virtual GradientVertexData GetGradientVertexData() => default;

        internal virtual void ApplyGradientSettings(GradientType gradientType, float gradientRotation) { }

        public abstract void Dispose();
    }
}