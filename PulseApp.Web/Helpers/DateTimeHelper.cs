using PulseApp.Protos;
using System;
using System.Linq;

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

        public static DateTime FirstDay(int year, int month = 1)
        {
            return new DateTime(year, month, 1);
        }

        public static DateTime FirstDay()
        {
            return new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }

        public static DateTime LastDay(int year, int month = 12)
        {
            return new DateTime(year, month, DateTime.DaysInMonth(year, month));
        }

        public static Tuple<DateTime, DateTime> MonthRange(int year, int month)
        {
            var start = new DateTime(year, month, 1);
            var end = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            return new Tuple<DateTime, DateTime>(start, end);
        }

        public static Tuple<DateTime, DateTime> YearRange(int year)
        {
            var end = new DateTime(year, 12, 31);
            var start = new DateTime(year, 1, 1);

            return new Tuple<DateTime, DateTime>(start, end);
        }

        public static int[] OffDaysOfMonth(DateTime[] offDays, int year, int month)
        {
            return offDays.Where(d => d.Year == year && d.Month == month).Select(d => d.Day).ToArray();
        }

        public static Date ToDate(this DateTime date)
        {
            return new Date() { Year = date.Year, Month = date.Month, Day = date.Day };
        }

        public static DateTime ToDateTime(this Date date)
        {
            return new DateTime(date.Year, date.Month, date.Day);
        }

    }
}

