using System;
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

        [SerializeField] private ThemeComponent _themeComponent;
        [SerializeField] private ToggleColorMapping _colorMapping;

        [SerializeField] private Image _targetImage;
        [SerializeField] private ToggleSpriteMapping _spriteMapping;

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
            if (!_colorMapping || !_themeComponent)
                return;

            _colorMapping.SetComponentForState(state, _themeComponent);
        }

        private readonly void ApplySpriteTransition(ToggleStateType state)
        {
            if (!_targetImage || !_spriteMapping)
                return;

            _targetImage.overrideSprite = _spriteMapping.GetSpriteForState(state);
        }
    }
}