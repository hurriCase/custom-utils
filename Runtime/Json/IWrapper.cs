using JetBrains.Annotations;

namespace CustomUtils.Runtime.Json
{
    /// <summary>
    /// Defines a wrapper for array data that can be serialized and deserialized.
    /// </summary>
    /// <typeparam name="T">The type of items in the array.</typeparam>
    [PublicAPI]
    public interface IWrapper<T>
    {
        /// <summary>
        /// Gets the array of items.
        /// </summary>

        T[] Items { get; }

        /// <summary>
        /// Sets the array of items.
        /// </summary>
        /// <param name="items">The array of items to set.</param>
        void SetItems(T[] items);
    }
}