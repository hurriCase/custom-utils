#if MULTI_THEME
#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using UnityEngine;
using Debug = UnityEngine.Debug;
#endif

namespace CustomUtils.Runtime.UI.Theme
{
    /// <summary>
    /// Detects Android system theme settings (Dark Mode / Light Mode).
    /// </summary>
    internal static class AndroidThemeDetector
    {
        /// <summary>
        /// Gets the current Android system theme setting.
        /// </summary>
        /// <returns>ThemeType.Dark if dark mode is enabled, ThemeType.Light otherwise</returns>
        internal static ThemeType GetAndroidSystemTheme()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var resources = currentActivity.Call<AndroidJavaObject>("getResources");
                using var configuration = resources.Call<AndroidJavaObject>("getConfiguration");

                var uiMode = configuration.Get<int>("uiMode");
                var nightModeFlags = uiMode & 0x30; // UI_MODE_NIGHT_MASK

                return nightModeFlags == 0x20 ? ThemeType.Dark : ThemeType.Light; // UI_MODE_NIGHT_YES
            }
            catch (Exception ex)
            {
                Debug.LogError("[AndroidThemeDetector::GetAndroidSystemTheme] " +
                               $"Failed to detect Android theme: {ex.Message}");
                return ThemeType.Light;
            }
#endif
            return ThemeType.Light;
        }
    }
}
#endif