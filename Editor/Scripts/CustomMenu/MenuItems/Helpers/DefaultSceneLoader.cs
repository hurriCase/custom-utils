using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable UnusedMember.Global
namespace CustomUtils.Editor.Scripts.CustomMenu.MenuItems.Helpers
{
    [InitializeOnLoad]
    public static class DefaultSceneLoader
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public static string EnableSetPlayModeSceneKey =>
            $"{PlayerSettings.applicationIdentifier}.enableSetPlayModeScene";

        private static bool IsChangePlayModeScene
        {
            get => EditorPrefs.GetBool(EnableSetPlayModeSceneKey, false);
            set => EditorPrefs.SetBool(EnableSetPlayModeSceneKey, value);
        }

        static DefaultSceneLoader()
        {
            EditorApplication.delayCall += ChangePlayModeScene;
            EditorApplication.quitting += OnEditorQuitting;

            ChangePlayModeScene();
        }

        public static void ToggleAutoLoad()
        {
            IsChangePlayModeScene = !IsChangePlayModeScene;

            Debug.Log($"Auto load startup scene is now {(IsChangePlayModeScene ? "enabled" : "disabled")}");

            ChangePlayModeScene();
        }

        public static bool IsDefaultSceneSet() => EditorPrefs.GetBool(EnableSetPlayModeSceneKey, false);

        private static void OnEditorQuitting()
        {
            EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
            EditorApplication.quitting -= OnEditorQuitting;
        }

        private static void ChangePlayModeScene()
        {
            EditorSceneManager.playModeStartScene = IsChangePlayModeScene
                ? CustomMenuSettings.Instance.DefaultSceneAsset
                : null;
        }

        private static void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            ChangePlayModeScene();
        }
    }
}