using System;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Effects
{
    /// <summary>
    /// Hard drop shadow of the shape, drawn under the fill in the same draw call.
    /// </summary>
    [Serializable]
    [PublicAPI]
    public struct ProceduralShadow
    {
        /// <summary>
        /// Gets whether the shadow is drawn.
        /// </summary>
        [field: SerializeField] public bool IsEnabled { get; private set; }

        /// <summary>
        /// Gets the shadow offset in pixels.
        /// </summary>
        [field: SerializeField] public Vector2 Offset { get; private set; }

        /// <summary>
        /// Gets the shadow color.
        /// </summary>
        [field: SerializeField] public Color Color { get; private set; }

        public ProceduralShadow(bool isEnabled, Vector2 offset, Color color)
        {
            IsEnabled = isEnabled;
            Offset = offset;
            Color = color;
        }
    }
}
