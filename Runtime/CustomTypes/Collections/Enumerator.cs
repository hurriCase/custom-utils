using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.CustomTypes.Collections
{
    /// <summary>
    /// Provides a struct-based enumerator for iterating over an array of elements.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the array.</typeparam>
    [PublicAPI]
    public struct Enumerator<TValue> : IEnumerator<TValue>
    {
        private readonly Entry<TValue>[] _array;
        private int _index;

        /// <summary>
        /// Initializes a new instance of the Enumerator struct.
        /// </summary>
        /// <param name="array">The array to enumerate over.</param>
        /// <exception cref="ArgumentNullException">Thrown when the array parameter is null.</exception>
        internal Enumerator(Entry<TValue>[] array)
        {
            _array = array ?? throw new ArgumentNullException(nameof(array));
            _index = -1;
        }

        /// <summary>
        /// Gets the current element in the enumeration.
        /// </summary>
        public readonly TValue Current
        {
            get
            {
                if (_index < 0 || _index >= _array.Length)
                    throw new InvalidOperationException("Enumerator is not positioned on a valid element.");
                return _array[_index].Value;
            }
        }

        /// <summary>
        /// Advances the enumerator to the next element.
        /// </summary>
        public bool MoveNext()
        {
            _index++;
            return _index < _array.Length;
        }

        /// <summary>
        /// Resets the enumerator to its initial position.
        /// </summary>
        public void Reset()
        {
            _index = -1;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Dispose()
        {
            // No cleanup needed for struct enumerator
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly object IEnumerator.Current => Current;
    }
}