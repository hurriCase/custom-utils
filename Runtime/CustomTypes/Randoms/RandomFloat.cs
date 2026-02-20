using System;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustomUtils.Runtime.CustomTypes.Randoms
{
    /// <summary>
    /// Represents a random float value generator with configurable minimum and maximum bounds.
    /// </summary>
    [Serializable]
    [PublicAPI]
    public struct RandomFloat
    {
        [SerializeField] private float _randomStart;
        [SerializeField] private float _randomEnd;

        /// <summary>
        /// Gets a random float value between the specified minimum and maximum bounds.
        /// </summary>
        /// <value>A random float value in the range [_randomStart, _randomEnd].</value>
        public float Value => Random.Range(_randomStart, _randomEnd);

        /// <summary>
        /// Initializes a new instance of the RandomFloat struct with the specified range.
        /// </summary>
        /// <param name="randomStart">The minimum value (inclusive) for the random range.</param>
        /// <param name="randomEnd">The maximum value (inclusive) for the random range.</param>
        public RandomFloat(float randomStart, float randomEnd)
        {
            _randomStart = randomStart;
            _randomEnd = randomEnd;
        }
    }
}