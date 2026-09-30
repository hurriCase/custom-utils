using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Compilation;
using UnityEngine;
using ZLinq;

// ReSharper disable UnusedMember.Global
namespace CustomUtils.Editor.Scripts.CustomMenu.MenuItems.Helpers
{
    public static class ScriptingSymbolHandler
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.delayCall += SyncAllSymbols;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static void SyncAllSymbols()
        {
            var settings = CustomMenuSettings.Instance;
            if (settings.ScriptingSymbols is null || settings.ScriptingSymbols.Length == 0)
                return;

            foreach (var symbol in settings.ScriptingSymbols)
                SyncSymbolWithPrefs(symbol.MenuTarget, symbol.GetPrefsKey());
        }

        public static void ToggleSymbol(string symbolName, string prefsKey)
        {
            var isEnabled = EditorPrefs.GetBool(prefsKey, false);
            isEnabled = !isEnabled;
            EditorPrefs.SetBool(prefsKey, isEnabled);

            if (isEnabled)
            {
                AddDefineSymbol(symbolName);
                Debug.Log($"[ScriptingSymbolHandler] Symbol '{symbolName}' enabled");
            }
            else
            {
                RemoveDefineSymbol(symbolName);
                Debug.Log($"[ScriptingSymbolHandler] Symbol '{symbolName}' disabled");
            }
        }

        public static bool IsSymbolEnabled(string prefsKey, bool defaultValue = false) =>
            EditorPrefs.GetBool(prefsKey, defaultValue);

        private static string GetCurrentDefineSymbols()
        {
            var activeBuildProfile = BuildProfile.GetActiveBuildProfile();
            if (activeBuildProfile)
                return activeBuildProfile.scriptingDefines is null
                    ? string.Empty
                    : string.Join(";", activeBuildProfile.scriptingDefines);

            var currentBuildTarget =
                NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            return PlayerSettings.GetScriptingDefineSymbols(currentBuildTarget);
        }

        private static void SetCurrentDefineSymbols(string updatedDefines)
        {
            var activeBuildProfile = BuildProfile.GetActiveBuildProfile();
            if (activeBuildProfile)
            {
                activeBuildProfile.scriptingDefines = updatedDefines.Split(';');
                EditorUtility.SetDirty(activeBuildProfile);
                AssetDatabase.SaveAssets();
                CompilationPipeline.RequestScriptCompilation();
                Debug.LogWarning($"[{nameof(ScriptingSymbolHandler)}::{nameof(SetCurrentDefineSymbols)}]" +
                                 $" Updated defines on active build profile '{activeBuildProfile.name}'" +
                                 " and requested recompilation");
                return;
            }

            var currentBuildTarget =
                NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            PlayerSettings.SetScriptingDefineSymbols(currentBuildTarget, updatedDefines);
        }

        private static void SyncSymbolWithPrefs(string symbolName, string prefsKey)
        {
            var isEnabled = EditorPrefs.GetBool(prefsKey, false);
            var currentDefines = GetCurrentDefineSymbols();
            var symbolDefined = currentDefines.Contains(symbolName);

            switch (isEnabled)
            {
                case true when !symbolDefined:
                    AddDefineSymbol(symbolName);
                    break;

                case false when symbolDefined:
                    RemoveDefineSymbol(symbolName);
                    break;
            }
        }

        private static void AddDefineSymbol(string symbolToAdd)
        {
            if (string.IsNullOrEmpty(symbolToAdd))
                return;

            var currentDefines = GetCurrentDefineSymbols();

            if (currentDefines.Contains(symbolToAdd))
                return;

            var updatedDefines = string.IsNullOrEmpty(currentDefines)
                ? symbolToAdd
                : currentDefines + ";" + symbolToAdd;

            SetCurrentDefineSymbols(updatedDefines);
        }

        private static void RemoveDefineSymbol(string symbolToRemove)
        {
            if (string.IsNullOrEmpty(symbolToRemove))
                return;

            var currentDefines = GetCurrentDefineSymbols();

            if (!currentDefines.Contains(symbolToRemove))
                return;

            var definesList = currentDefines.Split(';');

            var updatedDefines = string.Join(";", definesList
                .Where(defineSymbol => defineSymbol != symbolToRemove)
                .ToArray());

            SetCurrentDefineSymbols(updatedDefines);
        }
    }
}