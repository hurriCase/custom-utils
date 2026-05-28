using System;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Theme.ColorModifiers.Base
{
    internal abstract class ColorModifierBase : MonoBehaviour, IDisposable
    {
        internal abstract void UpdateColor(string guid, bool isForce = false);
        public abstract void Dispose();
    }
}