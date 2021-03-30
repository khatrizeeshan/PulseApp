using PulseApp.Helpers;
using PulseApp.Protos;
using System;
using System.Linq.Expressions;

namespace PulseApp.VMs
{
    public class EmployeeCalendarVM
    {
        public int CalendarId { get; set; }

        public string CalendarName { get; set; }

        public DateTime StartDate { get; set; }

        public static Expression<Func<EmployeeCalendarProto, EmployeeCalendarVM>> Selector = e => new EmployeeCalendarVM
        {
            CalendarId = e.CalendarId,
            CalendarName = e.CalendarName,
            StartDate = e.StartDate.ToDateTime(),
        };
    }
}
