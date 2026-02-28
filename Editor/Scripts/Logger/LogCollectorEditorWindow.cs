using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Logger;
using UnityEditor;

namespace CustomUtils.Editor.Scripts.Logger
{
    /// <inheritdoc />
    /// <summary>
    /// Custom editor window that allows configuring LogCollector settings.
    /// </summary>
    public sealed class LogCollectorEditorWindow : WindowBase
    {
        protected override void InitializeWindow()
        {
            serializedObject = new SerializedObject(LogCollectorSettings.Instance);
        }

        [MenuItem(MenuItemNames.LoggerSettingsMenuName)]
        public static void ShowWindow()
        {
            GetWindow<LogCollectorEditorWindow>(nameof(LogCollectorEditorWindow).ToSpacedWords());
        }

        protected override void DrawWindowContent()
        {
            EditorVisualControls.H1Label("Log Collector Settings");

            EditorGUI.BeginChangeCheck();

            PropertyField(nameof(LogCollectorSettings.MaxLogEntries));
            PropertyField(nameof(LogCollectorSettings.CaptureStackTrace));
            PropertyField(nameof(LogCollectorSettings.AutoStartCollect));
            PropertyField(nameof(LogCollectorSettings.MinimumLogLevel));

            if (!EditorGUI.EndChangeCheck())
                return;

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(LogCollectorSettings.Instance);
            AssetDatabase.SaveAssets();
        }
    }
}