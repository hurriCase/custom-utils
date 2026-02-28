using CustomUtils.Runtime.Other;
using CustomUtils.Runtime.UI.Theme;
using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.Selectables.Toggles.Mappings
{
    [CreateAssetMenu(
        fileName = nameof(ToggleColorMapping),
        menuName = ResourcePaths.MappingsPath + nameof(ToggleColorMapping)
    )]
    public sealed class ToggleColorMapping : ThemeStateMappingGeneric<ToggleStateType> { }
}