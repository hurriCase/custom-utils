using System;
using CustomUtils.Runtime.Storage;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.CustomMenu.MenuItems.MenuItems.MethodExecution
{
    [PublicAPI]
    public sealed class StorageHelper
    {
        public static async UniTask<bool> TryDeleteAllAsync()
        {
            try
            {
                var provider = ServiceProvider.Provider;
                var success = await provider.TryDeleteAllAsync();

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
        }
    }
}