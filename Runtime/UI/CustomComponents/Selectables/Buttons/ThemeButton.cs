using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents.Selectables.Buttons
{
    [PublicAPI]
    public class ThemeButton : Button
    {
        [field: SerializeField] public TextMeshProUGUI Text { get; private set; }
        [field: SerializeField] public Image Image { get; private set; }
        [field: SerializeField] public List<ThemeGraphicMapping> GraphicMappings { get; set; }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            ApplyGraphics(state);
        }

        private void ApplyGraphics(SelectionState state)
        {
            if (GraphicMappings is null || GraphicMappings.Count == 0)
                return;

            var mappedState = MapSelectionStateToSelectableState(state);

            foreach (var graphicMapping in GraphicMappings)
                graphicMapping.ApplyColor(mappedState);
        }

        private SelectableStateType MapSelectionStateToSelectableState(SelectionState state) =>
            state switch
            {
                SelectionState.Normal => SelectableStateType.Normal,
                SelectionState.Highlighted => SelectableStateType.Highlighted,
                SelectionState.Pressed => SelectableStateType.Pressed,
                SelectionState.Selected => SelectableStateType.Selected,
                SelectionState.Disabled => SelectableStateType.Disabled,
                _ => SelectableStateType.Normal
            };
    }
}