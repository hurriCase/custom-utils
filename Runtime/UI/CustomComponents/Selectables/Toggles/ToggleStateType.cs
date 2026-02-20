using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.CustomComponents.Selectables.Toggles
{
    [PublicAPI]
    public enum ToggleStateType
    {
        Normal = 0,
        Highlighted = 1,
        Pressed = 2,
        Selected = 3,
        Disabled = 4,
        On = 5
    }
}