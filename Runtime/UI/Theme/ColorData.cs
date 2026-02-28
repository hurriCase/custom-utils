using System;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Theme
{
    /// <summary>
    /// Represents color data containing the color type and name for theme system lookups.
    /// This structure is used to identify and retrieve specific colors from theme databases.
    /// </summary>
    [Serializable]
    [PublicAPI]
    public struct ColorData : IEquatable<ColorData>
    {
        /// <summary>
        /// Gets the type of color (solid, gradient, and so on) that determines which color modifier to use.
        /// </summary>
        [field: SerializeField] public ColorType ColorType { get; private set; }

        /// <summary>
        /// Gets the name of the color used for lookups in the theme color database.
        /// </summary>
        [field: SerializeField, ThemeColorName] public string ColorName { get; private set; }

        public bool Equals(ColorData other) => ColorType == other.ColorType && ColorName == other.ColorName;
        public override bool Equals(object obj) => obj is ColorData other && Equals(other);
        public readonly override int GetHashCode() => HashCode.Combine((int)ColorType, ColorName);
        public static bool operator ==(ColorData left, ColorData right) => left.Equals(right);
        public static bool operator !=(ColorData left, ColorData right) => !left.Equals(right);
    }
}