using System;
using CustomUtils.Runtime.UI.Theme;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.Locks
{
    /// <summary>
    /// Theme color a graphic shows while its selectable is locked.
    /// </summary>
    [Serializable]
    [PublicAPI]
    public struct LockedColorMapping
    {
        [SerializeField] private ThemeComponent _target;
        [SerializeField] private ColorData _lockedColor;

        /// <summary>
        /// Shows the locked color on the target, or its own color again.
        /// </summary>
        /// <param name="isLocked">Whether the selectable is locked.</param>
        public readonly void SetLocked(bool isLocked)
        {
            if (isLocked)
            {
                _target.SetColorOverride(_lockedColor);
                return;
            }

            _target.ClearColorOverride();
        }
    }
}
