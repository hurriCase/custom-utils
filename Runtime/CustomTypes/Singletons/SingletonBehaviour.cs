using System;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.CustomTypes.Singletons
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for MonoBehaviours that follow the Singleton pattern.
    /// Ensures only one instance exists and automatically handles cleanup.
    /// </summary>
    /// <typeparam name="T">The type of MonoBehaviour to make a singleton.</typeparam>
    [PublicAPI]
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        public static event Action OnDestroyed;

#if UNITY_EDITOR
        static SingletonBehaviour()
        {
            SingletonResetter.RegisterResetAction(static () =>
            {
                Instance = null;
                OnDestroyed = null;
            });
        }
#endif

        protected virtual void Awake()
        {
            if (Instance)
                Destroy(gameObject);

            Instance = this as T;
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
                return;

            Instance = null;

            OnDestroyed?.Invoke();
            OnDestroyed = null;
        }
    }
}