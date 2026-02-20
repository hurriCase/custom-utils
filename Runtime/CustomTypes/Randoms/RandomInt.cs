using System;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustomUtils.Runtime.CustomTypes.Randoms
{
    /// <summary>
    /// Represents a random integer value generator with configurable minimum and maximum bounds.
    /// </summary>
    [Serializable]
    [PublicAPI]
    public struct RandomInt
    {
        [SerializeField] private int _randomStart;
        [SerializeField] private int _randomEnd;

        /// <summary>
        /// Gets a random integer value between the specified minimum and maximum bounds.
        /// </summary>
        /// <value>A random integer value in the range [_randomStart, _randomEnd).</value>
        public int RandomValue => Random.Range(_randomStart, _randomEnd);

        /// <summary>
        /// Initializes a new instance of the RandomInt struct with the specified range.
        /// </summary>
        /// <param name="randomStart">The minimum value (inclusive) for the random range.</param>
        /// <param name="randomEnd">The maximum value (exclusive) for the random range.</param>
        public RandomInt(int randomStart, int randomEnd)
        {
            _randomStart = randomStart;
            _randomEnd = randomEnd;
        }
    }
}