using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CustomUtils.Runtime.Other;
using CustomUtils.Unsafe;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if MEMORY_PACK_INSTALLED
using MemoryPack;
#endif

namespace CustomUtils.Runtime.CustomTypes.Collections
{
    /// <summary>
    /// A generic class that associates an array of values with an underlying enum type as keys.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to be used as keys for this structure.</typeparam>
    /// <typeparam name="TValue">The type of values to be stored in the array.</typeparam>
    [PublicAPI]
    [Serializable]
#if MEMORY_PACK_INSTALLED
    [MemoryPackable]
#endif
    public partial class EnumArray<TEnum, TValue> : IEnumerable<TValue>, IEquatable<EnumArray<TEnum, TValue>>
        where TEnum : unmanaged, Enum
    {
        /// <summary>
        /// Gets the array of entries associated with the underlying enum type as keys.
        /// </summary>
        [field: SerializeField] public Entry<TValue>[] Entries { get; private set; }

        /// <summary>
        /// Gets all enum keys used in this array.
        /// </summary>
        public static TEnum[] CachedKeys => _cachedKeys;

        /// <summary>
        /// Gets the total number of elements in the array associated with the underlying enum type.
        /// </summary>
#if MEMORY_PACK_INSTALLED
        [MemoryPackIgnore]
#endif
        public int Length => Entries.Length;

        /// <summary>
        /// Gets all enum keys used in this array.
        /// </summary>
#if MEMORY_PACK_INSTALLED
        [MemoryPackIgnore]
#endif
        public IReadOnlyList<TEnum> Keys => _cachedKeys;

        /// <summary>
        /// Gets all key-value pairs from this array.
        /// </summary>
        /// <returns>A new array containing all key-value pairs with current values.</returns>
#if MEMORY_PACK_INSTALLED
        [MemoryPackIgnore]
#endif
        public KeyValuePair<TEnum, TValue>[] KeyValuePairs
        {
            get
            {
                var keys = _cachedKeys;
                var result = new KeyValuePair<TEnum, TValue>[keys.Length];
                for (var i = 0; i < keys.Length; i++)
                    result[i] = new KeyValuePair<TEnum, TValue>(keys[i], Entries[i].Value);
                return result;
            }
        }

#if MEMORY_PACK_INSTALLED
        [MemoryPackIgnore]
#endif
        private static TEnum[] _cachedKeys = GetUniqueKeys();

#if MEMORY_PACK_INSTALLED
        static partial void StaticConstructor()
        {
            StaticResetter.RegisterResetAction(static () => _cachedKeys = GetUniqueKeys());
        }
#else
        static EnumArray()
        {
            StaticResetter.RegisterResetAction(static () => _cachedKeys = GetUniqueKeys());
        }
#endif

        /// <summary>
        /// Initializes a new instance of the EnumArray with default values for all elements.
        /// </summary>
        public EnumArray()
        {
            ValidateEnumOrdering();

            Entries = new Entry<TValue>[_cachedKeys.Length];

            for (var i = 0; i < Entries.Length; i++)
                Entries[i] = new Entry<TValue>();
        }

        /// <summary>
        /// Initializes a new instance of the EnumArray with all elements set to the specified default value.
        /// </summary>
        /// <param name="defaultValue">The default value to assign to all elements in the array.</param>
        public EnumArray(TValue defaultValue)
        {
            ValidateEnumOrdering();

            Entries = new Entry<TValue>[_cachedKeys.Length];

            for (var i = 0; i < Entries.Length; i++)
                Entries[i] = new Entry<TValue> { Value = defaultValue };
        }

        /// <summary>
        /// Initializes a new instance of the EnumArray
        /// with all elements set to values created by the specified factory method.
        /// </summary>
        /// <param name="factory">A factory method that creates default values for each array element.</param>
        public EnumArray(Func<TValue> factory)
        {
            ValidateEnumOrdering();

            Entries = new Entry<TValue>[_cachedKeys.Length];

            for (var i = 0; i < Entries.Length; i++)
                Entries[i] = new Entry<TValue> { Value = factory() };
        }

#if MEMORY_PACK_INSTALLED
        [MemoryPackConstructor]
#endif
        [EditorBrowsable(EditorBrowsableState.Never)]
        public EnumArray(Entry<TValue>[] entries)
        {
            ValidateEnumOrdering();

            Entries = entries;
        }

        /// <summary>
        /// Indexer property that allows accessing or modifying values in the array
        /// using the associated enum type as the key.
        /// </summary>
        /// <param name="key">The enum key corresponding to the value in the array.</param>
        /// <returns>The value stored in the array at the position corresponding to the enum key.</returns>
        public TValue this[TEnum key]
        {
            get => Entries[UnsafeEnumConverter<TEnum>.ToInt32(key)].Value;
            set => Entries[UnsafeEnumConverter<TEnum>.ToInt32(key)].Value = value;
        }

        /// <summary>
        /// Indexer property that allows accessing or modifying values in the array
        /// using the index of the associated enum value.
        /// </summary>
        /// <param name="index">The index corresponding to the value in the array.</param>
        /// <returns>The value stored in the array at the position corresponding to the index.</returns>
        public TValue this[int index]
        {
            get => Entries[index].Value;
            set => Entries[index].Value = value;
        }

        /// <summary>
        /// Clears all entries in the array, setting them to their default values.
        /// </summary>
        public void Clear()
        {
            Array.Clear(Entries, 0, Entries.Length);
        }

        /// <summary>
        /// Enumerates over (key, value) tuples like a dictionary without allocations.
        /// </summary>
        /// <returns>A struct enumerator that iterates through key-value pairs.</returns>
        public TupleEnumerator<TEnum, TValue> AsTuples() => new(this);

        /// <summary>
        /// Returns a struct enumerator for iterating through the array of values.
        /// </summary>
        /// <returns>A struct enumerator for the array of values.</returns>
        public Enumerator<TValue> GetEnumerator() => new(Entries);

        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => GetEnumerator();

        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Determines whether the specified EnumArray is equal to the current EnumArray.
        /// </summary>
        /// <param name="other">The EnumArray to compare with the current instance.</param>
        /// <returns>true if the specified EnumArray is equal to the current instance; otherwise, false.</returns>
        public bool Equals(EnumArray<TEnum, TValue> other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (Entries.Length != other.Entries.Length)
                return false;

            for (var i = 0; i < Entries.Length; i++)
            {
                if (EqualityComparer<TValue>.Default.Equals(Entries[i].Value, other.Entries[i].Value) is false)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current EnumArray.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>true if the specified object is equal to the current instance; otherwise, false.</returns>
        public override bool Equals(object obj) =>
            obj is EnumArray<TEnum, TValue> other && Equals(other);

        /// <summary>
        /// Returns the hash code for the current EnumArray instance.
        /// </summary>
        /// <returns>A hash code for the current instance.</returns>
        public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

        /// <summary>
        /// Determines whether two EnumArray instances are equal.
        /// </summary>
        public static bool operator ==(EnumArray<TEnum, TValue> left, EnumArray<TEnum, TValue> right) =>
            Equals(left, right);

        /// <summary>
        /// Determines whether two EnumArray instances are not equal.
        /// </summary>
        public static bool operator !=(EnumArray<TEnum, TValue> left, EnumArray<TEnum, TValue> right) =>
            Equals(left, right) is false;

        private static TEnum[] GetUniqueKeys()
        {
            var allValues = (TEnum[])Enum.GetValues(typeof(TEnum));
            var count = 1;

            for (var i = 1; i < allValues.Length; i++)
            {
                if (UnsafeEnumConverter<TEnum>.ToInt32(allValues[i])
                    != UnsafeEnumConverter<TEnum>.ToInt32(allValues[i - 1]))
                    allValues[count++] = allValues[i];
            }

            return allValues.Length == 0 ? allValues : allValues[..count];
        }

        [Conditional("UNITY_EDITOR")]
        private static void ValidateEnumOrdering()
        {
            for (var i = 0; i < _cachedKeys.Length; i++)
            {
                var ordinal = UnsafeEnumConverter<TEnum>.ToInt32(_cachedKeys[i]);
                if (ordinal == i)
                    continue;

                Debug.LogError("[EnumArray::ValidateEnumOrdering] " +
                               $"{typeof(TEnum).Name} requires zero-based contiguous values. " +
                               $"{_cachedKeys[i]} has ordinal {ordinal}, expected {i}.");
            }
        }
    }
}