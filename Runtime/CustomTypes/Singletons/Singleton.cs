using System;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.CustomTypes.Singletons
{
    /// <summary>
    /// Base class for non-MonoBehaviour singletons.
    /// The instance is created lazily via <see cref="Activator.CreateInstance(Type)"/> on first access.
    /// </summary>
    /// <typeparam name="T">The type to make a singleton. Must be a class.</typeparam>
    [PublicAPI]
    public abstract class Singleton<T> where T : class
    {
        private static T _instance;

#if UNITY_EDITOR
        static Singleton()
        {
            SingletonResetter.RegisterResetAction(static () => _instance = null);
        }
#endif

        public static T Instance => _instance = _instance ?? (_instance = Activator.CreateInstance(typeof(T)) as T);
    }
}