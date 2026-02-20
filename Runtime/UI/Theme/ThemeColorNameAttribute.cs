using System;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Theme
{
    /// <inheritdoc />
    /// <summary>
    /// Property attribute used to mark string fields as theme color name fields.
    /// This attribute enables custom property drawers and validation for theme color names in the Unity Inspector.
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ThemeColorNameAttribute : PropertyAttribute { }
}