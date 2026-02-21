#if DATEONLY_INSTALLED
using System;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Constants
{
    /// <summary>
    /// Provides date-related constants and utilities.
    /// </summary>
    [PublicAPI]
    public static class Date
    {
        public const int DaysInCalendar = 42;
        public const int DayInMonths = 30;
        public const int DaysPerWeek = 7;

        public static DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
    }
}
#endif