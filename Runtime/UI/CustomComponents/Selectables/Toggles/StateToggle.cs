using System.Collections.Generic;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Extensions.Observables;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage;
using CustomUtils.Runtime.UI.CustomComponents.Selectables.Toggles.Mappings;
using JetBrains.Annotations;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents.Selectables.Toggles
{
    [PublicAPI]
    public class StateToggle : Toggle
    {
        [field: SerializeField] public TextMeshProUGUI Text { get; private set; }
        [field: SerializeField] public ProceduralImage Image { get; private set; }
        [field: SerializeField] public List<GameObject> CheckedObjects { get; private set; } = new();
        [field: SerializeField] public List<GameObject> UncheckedObjects { get; private set; } = new();
        [field: SerializeField] public List<ToggleGraphicMapping> AdditionalGraphics { get; set; } = new();

        protected override void Awake()
        {
            base.Awake();

            this.OnValueChangedAsObservable()
                .SubscribeUntilDestroy(this, static (isOn, toggle) => toggle.HandleStateChange(isOn));

            ApplyGraphics(currentSelectionState);
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            ApplyGraphics(state);
        }

        private void HandleStateChange(bool isOn)
        {
            SwitchObjects(isOn);
            ApplyGraphics(currentSelectionState);
        }

        private void SwitchObjects(bool isOn)
        {
            foreach (var checkedObject in CheckedObjects)
                checkedObject.AsNullable()?.SetActive(isOn);

            foreach (var uncheckedObject in UncheckedObjects)
                uncheckedObject.AsNullable()?.SetActive(!isOn);
        }

        private void ApplyGraphics(SelectionState state)
        {
            if (AdditionalGraphics.Count == 0)
                return;

            var mappedState = isOn ? ToggleStateType.On : MapSelectionStateToToggleState(state);

            foreach (var graphicMapping in AdditionalGraphics)
                graphicMapping.ApplyState(mappedState);
        }

        private ToggleStateType MapSelectionStateToToggleState(SelectionState state) =>
            state switch
            {
                SelectionState.Normal => ToggleStateType.Normal,
                SelectionState.Highlighted => ToggleStateType.Highlighted,
                SelectionState.Pressed => ToggleStateType.Pressed,
                SelectionState.Selected => ToggleStateType.Selected,
                SelectionState.Disabled => ToggleStateType.Disabled,
                _ => ToggleStateType.Normal
            };
    }
}