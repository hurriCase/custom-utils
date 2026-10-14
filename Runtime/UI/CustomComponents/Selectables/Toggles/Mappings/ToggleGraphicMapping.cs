using System;
using CustomUtils.Collections.Scripts;
using CustomUtils.Runtime.UI.Theme;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents.Selectables.Toggles.Mappings
{
    [Serializable]
    [PublicAPI]
    public struct ToggleGraphicMapping
    {
        [SerializeField] private Selectable.Transition _transitionType;

        [SerializeField] private ThemeStateMapping<ToggleStateType> _colorMapping;

        [SerializeField] private Image _targetImage;
        [SerializeField] private EnumArray<ToggleStateType, Sprite> _sprites;

        public void ApplyState(ToggleStateType state)
        {
            switch (_transitionType)
            {
                case Selectable.Transition.ColorTint:
                    ApplyColorTransition(state);
                    break;

                case Selectable.Transition.SpriteSwap:
                    ApplySpriteTransition(state);
                    break;
            }
        }

        private readonly void ApplyColorTransition(ToggleStateType state)
        {
            _colorMapping.SetState(state);
        }

        private readonly void ApplySpriteTransition(ToggleStateType state)
        {
            _targetImage.overrideSprite = _sprites[state];
        }
    }
}