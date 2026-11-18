using System;
using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.UI.Theme.Materials;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.Theme.ColorModifiers.Base
{
    internal abstract class ColorModifierBase : MonoBehaviour, IDisposable
    {
        [SerializeField, Self] private Graphic _graphic;

        protected Graphic Graphic => _graphic;

        internal void Init(Graphic graphic)
        {
            _graphic = graphic;
        }

        internal abstract void UpdateColor(string guid, bool isForce = false);

        internal virtual Material ModifyMaterial(Material baseMaterial) => baseMaterial;

        internal virtual GradientVertexData GetGradientVertexData(ThemeStyle style) => default;

        public abstract void Dispose();
    }
}