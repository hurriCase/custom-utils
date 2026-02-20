using System;
using CustomUtils.Runtime.Storage.Base;
using CustomUtils.Runtime.Storage.Providers;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Storage
{
    /// <summary>
    /// Provides platform-specific storage provider instances with lazy initialization.
    /// Automatically selects the appropriate provider based on the current platform.
    /// </summary>
    [PublicAPI]
    public static class ServiceProvider
    {
        private static readonly Lazy<IStorageProvider> _lazyProvider = new(CreateProvider);
        private static IStorageProvider _customProvider;

        /// <summary>
        /// Gets the current storage provider instance.
        /// Returns a platform-specific provider or a custom provider if one was set using SetProvider.
        /// </summary>
        /// <returns>The active storage provider instance.</returns>

        public static IStorageProvider Provider => _customProvider ?? _lazyProvider.Value;

        /// <summary>
        /// Sets a custom storage provider. Must be called before any storage operations.
        /// </summary>
        public static void SetProvider<TProvider>() where TProvider : class, IStorageProvider, new()
        {
            _customProvider = new TProvider();
        }

        /// <summary>
        /// Sets a custom storage provider instance. Must be called before any storage operations.
        /// </summary>
        public static void SetProvider(IStorageProvider provider)
        {
            _customProvider = provider;
        }

        private static IStorageProvider CreateProvider()
        {
            return
#if !UNITY_EDITOR && UNITY_ANDROID
                new BinaryFileProvider();
#elif CRAZY_GAMES
                new CrazyGamesStorageProvider();
#elif UNITY_EDITOR
                new PlayerPrefsProvider();
#else
                new PlayerPrefsProvider();
#endif
        }
    }
}