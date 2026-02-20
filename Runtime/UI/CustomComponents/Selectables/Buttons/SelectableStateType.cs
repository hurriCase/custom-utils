using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.CustomComponents.Selectables.Buttons
{
    [PublicAPI]
    public enum SelectableStateType
    {
        Normal = 0,
        Highlighted = 1,
        Pressed = 2,
        Selected = 3,
        Disabled = 4
    }
}