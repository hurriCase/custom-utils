using System;
using CustomUtils.Runtime.AssetLoader;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.CustomTypes.Singletons
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for MonoBehaviours that follow the Singleton pattern and persist between scenes.
    /// Can be instantiated from a prefab in Resources folder or created dynamically.
    /// </summary>
    /// <typeparam name="T">The type of MonoBehaviour to make a persistent singleton.</typeparam>
    [PublicAPI]
    public abstract class PersistentSingletonBehavior<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_created)
                    return _instance;

                _instance = CreateInstance();
                _created = true;

                return _instance;
            }
        }

        public static event Action OnDestroyed;

        // ReSharper disable once StaticMemberInGenericType | intentional, each T has its own creation state
        private static bool _created;

#if UNITY_EDITOR
        static PersistentSingletonBehavior()
        {
            SingletonResetter.RegisterResetAction(static () =>
            {
                _instance = null;
                OnDestroyed = null;
                _created = false;
            });
        }
#endif
        protected virtual void Awake()
        {
            if (!_instance)
            {
                _instance = this as T;
                _created = true;

                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
                Destroy(gameObject);
        }

        protected virtual void OnDestroy()
        {
            if (_instance != this)
                return;

            _instance = null;

            OnDestroyed?.Invoke();
        }

        private static T CreateInstance()
        {
            var type = typeof(T);
            var prefabName = type.Name;

            if (ResourceLoader<T>.TryLoad(out var prefab))
                prefabName = prefab.name;

            var gameObject = prefab ? Instantiate(prefab.gameObject) : new GameObject(prefabName);

            var component = gameObject.GetComponent<T>();
            return component ? component : gameObject.AddComponent<T>();
        }
    }
}