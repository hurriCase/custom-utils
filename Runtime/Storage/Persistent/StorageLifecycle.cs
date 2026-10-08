using UnityEngine;

namespace CustomUtils.Runtime.Storage.Persistent
{
    internal sealed class StorageLifecycle : MonoBehaviour
    {
        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
                StorageChangeTracker.FlushAll();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                StorageChangeTracker.FlushAll();
        }

        private void OnApplicationQuit()
        {
            StorageChangeTracker.FlushAll();
        }
    }
}
