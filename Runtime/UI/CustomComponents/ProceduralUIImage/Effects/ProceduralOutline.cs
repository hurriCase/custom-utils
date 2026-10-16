using System;
using CustomUtils.Runtime.UI.Theme;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Effects
{
    /// <summary>
    /// Outline drawn on top of the fill along the shape edge.
    /// </summary>
    [Serializable]
    [PublicAPI]
    public struct ProceduralOutline
    {
        /// <summary>
        /// Gets the outline width in pixels; zero disables the outline.
        /// </summary>
        [field: SerializeField, Min(0)] public float Width { get; private set; }

        /// <summary>
        /// Gets where the outline sits relative to the shape edge.
        /// </summary>
        [field: SerializeField] public OutlineAlignment Alignment { get; private set; }

        /// <summary>
        /// Gets the outline style: theme color, gradient direction and halftone.
        /// </summary>
        [field: SerializeField] public ThemeStyle Style { get; private set; }

        internal bool IsVisible => Width > 0f;

        internal float OutsideFraction => Alignment switch
        {
            OutlineAlignment.Center => 0.5f,
            OutlineAlignment.Outside => 1f,
            _ => 0f
        };

        public ProceduralOutline(float width, OutlineAlignment alignment, ThemeStyle style)
        {
            Width = Mathf.Max(0f, width);
            Alignment = alignment;
            Style = style;
        }
    }
}
