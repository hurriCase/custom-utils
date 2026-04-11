using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.CustomEditorUtilities
{
    internal abstract class MainToolbarSliderExample
    {
        private const float MinTimeScale = 0f;
        private const float MaxTimeScale = 100f;

        private const string TimeScaleToolbarPath = "TimeScale";
        private const string TimeScaleDisplayName = "Time Scale";

        private const string OverrideButtonDisplayName = "Override";
        private const string OverrideButtonTooltip = "If true overrides any timeScale change to specified in slider";

        private static float _currentTimeScale;
        private static bool _isOverride;

        private static MainToolbarLabel _timeScaleLabel;

        [MainToolbarElement(TimeScaleToolbarPath, defaultDockPosition = MainToolbarDockPosition.Middle)]
        internal static IEnumerable<MainToolbarElement> TimeSlider()
        {
            var sliderContent = new MainToolbarContent(TimeScaleDisplayName);

            yield return new MainToolbarSlider(
                sliderContent,
                Time.timeScale,
                MinTimeScale,
                MaxTimeScale,
                OnSliderValueChanged);

            var buttonContent = new MainToolbarContent(OverrideButtonDisplayName, OverrideButtonTooltip);
            yield return new MainToolbarToggle(buttonContent, false, ToggleOverride);
        }

        private static void ToggleOverride(bool isActive)
        {
            if (isActive)
            {
                EditorApplication.update += OverrideTimeScale;
                return;
            }

            EditorApplication.update -= OverrideTimeScale;
        }

        private static void OverrideTimeScale()
        {
            Time.timeScale = _currentTimeScale;
        }

        private static void OnSliderValueChanged(float newValue)
        {
            Time.timeScale = newValue;
            _currentTimeScale = newValue;
        }
    }
}