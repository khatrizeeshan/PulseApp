using PulseApp.Constants;
using PulseApp.Data;
using PulseApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulseApp.Helpers
{
    public static class CalendarHelper
    {
        public static CalendarDay[] MakeWeekends(this Calendar calendar, int year, int month)
        {
            var days = new List<CalendarDay>();

            var (start, end) = DateTimeHelper.YearRange(year, month);
            var date = start;

            var array = calendar.Weekends.ToCharArray();
            var on = '1';

            while (date <= end)
            {
                if (array[(int)date.DayOfWeek] == on)
                {
                    days.Add(new CalendarDay() { CalendarId = calendar.Id, Date = date, DayTypeId = DayTypes.Weekend });
                }

                date = date.AddDays(1);
            }

            return days.ToArray();
        }
    }
}
