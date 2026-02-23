#if CUSTOM_LOCALIZATION
using CustomUtils.Runtime.Localization;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="LocalizationKey"/>.
    /// </summary>
    [PublicAPI]
    public static class LocalizationKeyExtensions
    {
        /// <summary>
        /// Gets the localized string for the specified localization key.
        /// </summary>
        /// <param name="key">The localization key to get the localized string for.</param>
        /// <returns>The localized string corresponding to the key.</returns>
        public static string GetLocalization(this LocalizationKey key) => LocalizationController.Localize(key);
    }
}
#endif