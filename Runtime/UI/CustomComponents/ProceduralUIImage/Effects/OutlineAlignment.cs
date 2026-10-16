using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Effects
{
    /// <summary>
    /// Where the outline sits relative to the shape edge, matching Figma's stroke alignment.
    /// </summary>
    [PublicAPI]
    public enum OutlineAlignment
    {
        Inside = 0,
        Center = 1,
        Outside = 2
    }
}
