using JetBrains.Annotations;
using R3;

namespace CustomUtils.Runtime.UI.Theme
{
    /// <summary>
    /// Central handler for managing the current theme state across the application.
    /// </summary>
    [PublicAPI]
    public static class ThemeHandler
    {
        /// <summary>
        /// Gets the reactive property that holds the current theme type.
        /// Subscribe to this property to receive notifications when the theme changes.
        /// </summary>

        public static ReactiveProperty<ThemeType> CurrentThemeType { get; } = new(ThemeType.Light);
    }
}