using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.CustomTypes.Collections
{
    /// <summary>
    /// Provides an enumerator for iterating through key-value pairs where keys are enum values.
    /// </summary>
    /// <typeparam name="TEnum">The enumeration type that represents the keys.</typeparam>
    /// <typeparam name="TValue">The type of the associated values.</typeparam>
    [PublicAPI]
    public struct TupleEnumerator<TEnum, TValue> : IEnumerator<(TEnum Key, TValue Value)>
        where TEnum : unmanaged, Enum
    {
        private readonly EnumArray<TEnum, TValue> _enumArray;
        private readonly TEnum[] _enumValues;
        private int _index;

        /// <summary>
        /// Initializes a new instance of the TupleEnumerator struct.
        /// </summary>
        /// <param name="enumArray">The EnumArray instance to enumerate over.</param>
        internal TupleEnumerator(EnumArray<TEnum, TValue> enumArray)
        {
            _enumArray = enumArray;
            _enumValues = (TEnum[])Enum.GetValues(typeof(TEnum));
            _index = -1;
        }

        /// <summary>
        /// Gets the current element as a (key, value) tuple.
        /// </summary>
        public readonly (TEnum Key, TValue Value) Current
        {
            get
            {
                if (_index < 0 || _index >= _enumValues.Length)
                    throw new InvalidOperationException("Enumerator is not positioned on a valid element.");
                return (_enumValues[_index], _enumArray.Entries[_index].Value);
            }
        }

        /// <summary>
        /// Advances the enumerator to the next element.
        /// </summary>
        public bool MoveNext()
        {
            _index++;
            return _index < _enumValues.Length && _index < _enumArray.Entries.Length;
        }

        /// <summary>
        /// Resets the enumerator to its initial position.
        /// </summary>
        public void Reset() => _index = -1;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Dispose()
        {
            // No cleanup needed for struct enumerator
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public readonly TupleEnumerator<TEnum, TValue> GetEnumerator() => this;

        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly object IEnumerator.Current => Current;
    }
}