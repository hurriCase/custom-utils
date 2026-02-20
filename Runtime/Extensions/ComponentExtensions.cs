using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Component"/>.
    /// </summary>
    [PublicAPI]
    public static class ComponentExtensions
    {
        /// <summary>
        /// Attempts to find a component of type T in the parent objects of the specified component.
        /// </summary>
        /// <typeparam name="T">The type of component to search for, must inherit from Component.</typeparam>
        /// <param name="component">The component to start the search from.</param>
        /// <param name="result">When this method returns, contains the component of type T if found; otherwise, null.</param>
        /// <returns>
        /// <c>true</c> if a component of type T was found in the parent hierarchy; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryGetComponentInParent<T>(this Component component, out T result) where T : Component
        {
            result = component.GetComponentInParent<T>();
            return result;
        }

        /// <summary>
        /// Sets the active state of the GameObject associated with the specified component.
        /// </summary>
        /// <param name="component">The component whose associated GameObject's active state will be set.</param>
        /// <param name="isActive">A boolean value indicating whether to activate (<c>true</c>) or deactivate (<c>false</c>) the GameObject.</param>
        public static void SetActive(this Component component, bool isActive)
            => component.gameObject.SetActive(isActive);

        /// <summary>
        /// Determines whether the source component is rendered in front of the target component based on the sibling index.
        /// </summary>
        /// <typeparam name="TSource">The type of the source component, must inherit from Component.</typeparam>
        /// <typeparam name="TTarget">The type of the target component, must inherit from Component.</typeparam>
        /// <param name="component">The source component to compare.</param>
        /// <param name="target">The target component to compare against.</param>
        /// <returns>
        /// <c>true</c> if the source component has a higher sibling index than the target (appears in front);
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// In Unity's UI system, components with higher sibling indexes are rendered on top of those with lower indexes.
        /// This method compares the sibling indexes of the transforms associated with the components.
        /// </remarks>
        public static bool IsInFrontOf<TSource, TTarget>(this TSource component, TTarget target)
            where TSource : Component
            where TTarget : Component
            => component.transform.GetSiblingIndex() > target.transform.GetSiblingIndex();

        /// <summary>
        /// Retrieves a component of type T from the GameObject attached to the specified component,
        /// or adds one if it does not already exist.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve or add, must inherit from Component.</typeparam>
        /// <param name="component">The component whose GameObject is searched for the specified type.</param>
        /// <returns>The existing component of type T, or the newly added component if it did not exist.</returns>
        public static T GetOrAddComponent<T>(this Component component) where T : Component =>
            component.GetComponent<T>() ? component.GetComponent<T>() : component.gameObject.AddComponent<T>();

        /// <summary>
        /// Destroys the specified component instance.
        /// Uses immediate destruction if the application is not in play mode;
        /// otherwise, schedules the destruction at the end of the frame.
        /// </summary>
        /// <param name="component">The component instance to destroy.</param>
        public static void Destroy(this Component component)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(component);
                return;
            }

            Object.DestroyImmediate(component);
        }
    }
}