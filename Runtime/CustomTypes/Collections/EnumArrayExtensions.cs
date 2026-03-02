using System;
using JetBrains.Annotations;
using Random = UnityEngine.Random;

namespace CustomUtils.Runtime.CustomTypes.Collections
{
    /// <summary>
    /// Provides extension methods for <see cref="EnumArray{TEnum, TValue}"/>.
    /// </summary>
    [PublicAPI]
    public static class EnumArrayExtensions
    {
        /// <summary>
        /// Selects a random enum value based on weighted chances defined in the array.
        /// Uses accumulated probability to perform a single random roll.
        /// </summary>
        /// <param name="chancePerEntry">The enum array mapping each enum value to its chance weight.</param>
        /// <param name="fallback">The value to return if no entry matches the roll.</param>
        /// <typeparam name="TEnum">The enum type used as the key.</typeparam>
        /// <returns>A randomly selected enum value weighted by chance, or <paramref name="fallback"/> if none matched.</returns>
        public static TEnum RollByChance<TEnum>(this EnumArray<TEnum, int> chancePerEntry, TEnum fallback)
            where TEnum : unmanaged, Enum
        {
            var totalChance = chancePerEntry.Entries.Sum(static entry => entry.Value);
            var roll = Random.Range(0, totalChance);
            var accumulatedChance = 0f;

            foreach (var (enumValue, chance) in chancePerEntry.AsTuples())
            {
                accumulatedChance += chance;
                if (roll <= accumulatedChance)
                    return enumValue;
            }

            return fallback;
        }
    }
}