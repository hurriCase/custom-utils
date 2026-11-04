using System;
using CustomUtils.Runtime.Storage;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.CustomMenu.MenuItems.MenuItems.MethodExecution
{
    [PublicAPI]
    public static class StorageHelper
    {
        private static bool _isPlayModeRequested;

        public static async UniTask<bool> TryDeleteAllAsync()
        {
            EditorApplication.playModeStateChanged += PostponePlayMode;

            try
            {
                var success = await StorageProvider.Provider.TryDeleteAllAsync();

                if (success)
                    Debug.Log("[StorageHelper::TryDeleteAllAsync] Successfully cleared all data");
                else
                    Debug.LogWarning("[StorageHelper::TryDeleteAllAsync] Failed to clear all data - " +
                                     "provider doesn't support TryDeleteAll or operation failed");

                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError("[StorageHelper::TryDeleteAllAsync] " +
                               $"Unexpected error clearing data: {ex.Message}");
                return false;
            }
            finally
            {
                EditorApplication.playModeStateChanged -= PostponePlayMode;

                if (_isPlayModeRequested)
                    EditorApplication.isPlaying = true;

                _isPlayModeRequested = false;
            }
        }

        private static void PostponePlayMode(PlayModeStateChange stateChange)
        {
            if (stateChange != PlayModeStateChange.ExitingEditMode)
                return;

            EditorApplication.isPlaying = false;
            _isPlayModeRequested = true;
            Debug.LogWarning("[StorageHelper::PostponePlayMode] Stored data is still being deleted, " +
                             "play mode starts when it finishes");
        }
    }
}
