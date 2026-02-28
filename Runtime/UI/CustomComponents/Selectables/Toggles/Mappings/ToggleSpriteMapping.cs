using CustomUtils.Runtime.CustomTypes.Collections;
using CustomUtils.Runtime.Other;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.Selectables.Toggles.Mappings
{
    [PublicAPI]
    [CreateAssetMenu(
        fileName = nameof(ToggleSpriteMapping),
        menuName = ResourcePaths.MappingsPath + nameof(ToggleSpriteMapping)
    )]
    public sealed class ToggleSpriteMapping : ScriptableObject
    {
        [field: SerializeField] public EnumArray<ToggleStateType, Sprite> StateMappings { get; private set; }

        public Sprite GetSpriteForState(ToggleStateType state) => StateMappings[state];
    }
}