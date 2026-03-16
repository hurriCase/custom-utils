using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.GradientHelpers.Base
{
    [PublicAPI]
    public interface IGradientEffect<in TComponent> where TComponent : Component
    {
        void ApplyGradient(TComponent component, Gradient gradient, GradientDirection direction);
        void ClearGradient(TComponent component);
    }
}