using System;
using CustomUtils.Runtime.CustomTypes.Collections;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Theme
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for creating theme state mappings that associate enum states with color data.
    /// </summary>
    /// <typeparam name="TEnum">The enum type representing different states.</typeparam>
    [PublicAPI]
    public abstract class ThemeStateMappingGeneric<TEnum> : ScriptableObject where TEnum : unmanaged, Enum
    {
        /// <summary>
        /// Gets the state mappings that associate each enum state with its corresponding color data.
        /// </summary>
        [field: SerializeField] public EnumArray<TEnum, ColorData> StateMappings { get; private set; }

        /// <summary>
        /// Sets the color data for a specific state on the provided theme component.
        /// </summary>
        /// <param name="state">The state to apply.</param>
        /// <param name="themeComponent">The theme component to update.</param>
        public void SetComponentForState(TEnum state, ThemeComponent themeComponent)
        {
            themeComponent.UpdateColorData(StateMappings[state]);
        }
    }
}