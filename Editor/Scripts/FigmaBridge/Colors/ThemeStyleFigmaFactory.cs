using CustomUtils.Editor.Scripts.FigmaBridge.Colors.Converters;
using CustomUtils.Editor.Scripts.FigmaBridge.Colors.Data;
using CustomUtils.Runtime.UI.Theme;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Colors
{
    internal static class ThemeStyleFigmaFactory
    {
        // Figma has no diamond direction and no halftone settings when the fill has no pattern,
        // so those are kept from the current style.
        internal static ThemeStyle Create(ColorFigmaClipboardData clipboardData, ThemeStyle currentStyle)
        {
            var converter = FigmaColorConverterConfig.Instance.Converters[clipboardData.ColorType];
            var colorGuid = converter.GetOrAddColor(clipboardData);
            var colorData = new ColorData(clipboardData.ColorType, colorGuid);
            var halftone = clipboardData.Halftone;
            var useHalftone = halftone != null;
            var halftoneProperties = useHalftone ? halftone.ToHalftoneProperties() : currentStyle.HalftoneProperties;

            return new ThemeStyle(
                colorData,
                clipboardData.GradientType,
                clipboardData.Rotation,
                currentStyle.DiamondDirection,
                useHalftone,
                halftoneProperties);
        }
    }
}
