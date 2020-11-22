using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulseApp.Helpers
{
    public static class DateTimeHelper
    {
        public static string GetMonthName(int month)
        {
            return new DateTime(2020, month, 1).ToString("MMMM");
        }

        public static bool IsCurrentMonth(int year, int month)
        {
            var today = DateTime.Today;
            return today.Month == month && today.Year == year;
        }

        public static int[] WeekDays(int year, int month)
        {
            return Enumerable.Range(1, DateTime.DaysInMonth(year, month))
                    .Select(day => new DateTime(year, month, day))
                    .Where(dt => dt.DayOfWeek == DayOfWeek.Sunday ||
                                    dt.DayOfWeek == DayOfWeek.Saturday)
                    .Select(dt => dt.Day)
                    .ToArray();
        }

        public static DateTime FirstDay(int year, int month)
        {
            return new DateTime(year, month, 1);
        }
    }
}
