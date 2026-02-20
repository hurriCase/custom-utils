using System;
using CustomUtils.Runtime.UI.Theme;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.Selectables.Buttons
{
    [Serializable]
    [PublicAPI]
    public struct ThemeGraphicMapping
    {
        [SerializeField] private ThemeComponent _themeComponent;
        [SerializeField] private SelectableColorMapping _colorMapping;

        public void ApplyColor(SelectableStateType state)
        {
            if (!_colorMapping || !_themeComponent)
                return;

            _colorMapping.SetComponentForState(state, _themeComponent);
        }
    }
}