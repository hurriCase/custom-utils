using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.Theme
{
    /// <summary>
    /// Defines the available theme types for the UI theme system.
    /// </summary>
    [PublicAPI]
    public enum ThemeType
    {
        /// <summary>
        /// Light theme variant with bright colors and light backgrounds.
        /// </summary>
        Light = 0,

        /// <summary>
        /// Dark theme variant with dark colors and dark backgrounds.
        /// </summary>
        Dark = 1
    }
}