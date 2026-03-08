#if MULTI_THEME
using CustomUtils.Runtime.CustomTypes.Collections;
#endif

namespace CustomUtils.Runtime.UI.Theme.ThemeColors
{
    // ReSharper disable once TypeParameterCanBeVariant | we can't do it for MULTI_THEME case
    internal interface IThemeColor<TColor>
    {
        string Name { get; }
#if MULTI_THEME
        EnumArray<ThemeType, TColor> Colors { get; }
#else
        TColor Color { get; }
#endif
    }
}