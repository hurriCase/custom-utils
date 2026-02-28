using System.Collections.Generic;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Other
{
    [PublicAPI]
    public static class SetPropertyUtility
    {
        public static bool TrySetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
                return false;

            currentValue = newValue;
            return true;
        }
    }
}