using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="GameObject"/>.
    /// </summary>
    [PublicAPI]
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Retrieves a component of type T from the GameObject attached to the specified component,
        /// or adds one if it does not already exist.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve or add, must inherit from Component.</typeparam>
        /// <param name="gameObject">The gameObject is searched for the specified type.</param>
        /// <returns>The existing component of type T, or the newly added component if it did not exist.</returns>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            return component ? component : gameObject.AddComponent<T>();
        }
    }
}