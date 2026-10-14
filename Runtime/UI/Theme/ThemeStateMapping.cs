using System;
using System.Collections.Generic;
using CustomUtils.Collections.Scripts;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Theme
{
    /// <summary>
    /// Inline mapping that applies a <see cref="ThemeStyle"/> per enum state to its target theme components.
    /// </summary>
    /// <typeparam name="TEnum">The enum type representing different states.</typeparam>
    [Serializable]
    [PublicAPI]
    public sealed class ThemeStateMapping<TEnum> where TEnum : unmanaged, Enum
    {
        /// <summary>
        /// Gets the theme components updated by this mapping.
        /// </summary>
        [field: SerializeField] public List<ThemeComponent> Targets { get; private set; }

        /// <summary>
        /// Gets the style applied for each state.
        /// </summary>
        [field: SerializeField] public EnumArray<TEnum, ThemeStyle> Styles { get; private set; }

        /// <summary>
        /// Applies the style mapped to the given state to every target.
        /// </summary>
        /// <param name="state">The state to apply.</param>
        public void SetState(TEnum state)
        {
            var style = Styles[state];
            foreach (var target in Targets)
                target.UpdateStyle(style);
        }
    }
}
