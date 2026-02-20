using CustomUtils.Runtime.UI.Theme;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.Selectables.Buttons
{
    [CreateAssetMenu(
        fileName = nameof(SelectableColorMapping),
        menuName = ResourcePaths.MappingsPath + nameof(SelectableColorMapping)
    )]
    public sealed class SelectableColorMapping : ThemeStateMappingGeneric<SelectableStateType> { }
}