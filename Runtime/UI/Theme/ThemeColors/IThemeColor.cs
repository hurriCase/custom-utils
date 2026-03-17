#if MULTI_THEME
using CustomUtils.Runtime.CustomTypes.Collections;
#endif

namespace CustomUtils.Runtime.UI.Theme.ThemeColors
{
    // ReSharper disable once TypeParameterCanBeVariant | we can't do it for MULTI_THEME case
    internal interface IThemeColor<TColor>
    {
        string Name { get; }
        string Guid { get; }
#if MULTI_THEME
        EnumArray<ThemeType, TColor> Colors { get; }
#else
        TColor Color { get; }
#endif

#if UNITY_EDITOR
        void EnsureGuid();
#endif
    }
}