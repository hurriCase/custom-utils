using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
#if PRIME_TWEEN_EXPERIMENTAL
using CustomUtils.Runtime.Other;
#endif

namespace CustomUtils.Editor.Scripts.CustomEditorUtilities
{
    internal static class TimeScaleToolbar
    {
        private const float MinTimeScale = 0f;
        private const float MaxTimeScale = 10f;

        private const string ToolbarPath = "TimeScale";
        private const string DisplayName = "Time Scale";

        private const string OverrideTooltip = "If true overrides any timeScale change to specified in slider";
        private const string ResetTooltip = "Reset Time Scale to 1";

        private const string ResetIconName = "Refresh";
        private const string OverrideIconName = "LockIcon-On";

        private static float _timeScale = 1f;
        private static bool _isOverride;

#if PRIME_TWEEN_EXPERIMENTAL
        private static TimeScaleTweenDriver _tweenDriver;
#endif

        [MainToolbarElement(ToolbarPath, defaultDockPosition = MainToolbarDockPosition.Middle)]
        internal static IEnumerable<MainToolbarElement> CreateElements()
        {
            var sliderContent = new MainToolbarContent(DisplayName);
            yield return new MainToolbarSlider(sliderContent, Time.timeScale, MinTimeScale, MaxTimeScale, SetTimeScale);

            var resetIcon = EditorGUIUtility.IconContent(ResetIconName).image as Texture2D;
            var resetContent = new MainToolbarContent(resetIcon, ResetTooltip);
            yield return new MainToolbarButton(resetContent, ResetTimeScale);

            var overrideIcon = EditorGUIUtility.IconContent(OverrideIconName).image as Texture2D;
            var overrideContent = new MainToolbarContent(overrideIcon, OverrideTooltip);
            yield return new MainToolbarToggle(overrideContent, _isOverride, ToggleOverride);
        }

        private static void ToggleOverride(bool isActive)
        {
            _isOverride = isActive;

            if (isActive)
            {
                EditorApplication.update += OverrideTimeScale;
                return;
            }

            EditorApplication.update -= OverrideTimeScale;
        }

        private static void OverrideTimeScale()
        {
            Time.timeScale = _timeScale;
        }

        private static void ResetTimeScale()
        {
            SetTimeScale(1f);
            MainToolbar.Refresh(ToolbarPath);
        }

        private static void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
            _timeScale = timeScale;

#if PRIME_TWEEN_EXPERIMENTAL
            EnsureTweenDriver();

            if (_tweenDriver)
                _tweenDriver.UnscaledTimeScale = timeScale;
#endif
        }

#if PRIME_TWEEN_EXPERIMENTAL
        private static void EnsureTweenDriver()
        {
            if (!Application.isPlaying || _tweenDriver)
                return;

            var driverObject = new GameObject(nameof(TimeScaleTweenDriver));
            Object.DontDestroyOnLoad(driverObject);
            _tweenDriver = driverObject.AddComponent<TimeScaleTweenDriver>();
        }
#endif
    }
}
