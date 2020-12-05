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
        public static CalendarDay[] MakeWeekends(this Calendar calendar, string weekends)
        {
            var days = new List<CalendarDay>();
            var date = calendar.StartDate;

            var array = weekends.ToCharArray();
            var on = '1';

            while (date <= calendar.EndDate)
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
