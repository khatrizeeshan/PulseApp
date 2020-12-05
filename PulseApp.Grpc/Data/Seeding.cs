using PulseApp.Constants;
using PulseApp.Helpers;
using PulseApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulseApp.Data
{
    public class Seeding
    {
        private static readonly Setting Setting = new Setting() { Id = 0, Weekends = "1000000" };

        private static readonly DayType[] DayTypeList = new DayType[]
        {
            new DayType() { Id = DayTypes.Weekend, Code = "W", Name = "Weekend" },
            new DayType() { Id = DayTypes.Holiday, Code = "H", Name = "Holiday" },
        };

        private static readonly Calendar[] CalendarList = new Calendar[]
        {
            new Calendar() { StartDate = new DateTime(2019, 7, 1), EndDate = new DateTime(2020, 6, 30) },
            new Calendar() { StartDate = new DateTime(2020, 7, 1), EndDate = new DateTime(2021, 6, 30) },
        };

        private static readonly LeavePolicy[] LeavePolicyList = new LeavePolicy[]
        {
            new LeavePolicy() { Name = "Probation", Details = new List<LeavePolicyDetail>() },
            new LeavePolicy() { Name = "Permanent", Details = new List<LeavePolicyDetail> {  
                new LeavePolicyDetail() { LeaveTypeId = LeaveTypes.Casual, Count = 8 },
                new LeavePolicyDetail() { LeaveTypeId = LeaveTypes.Sick, Count = 8 },
                new LeavePolicyDetail() { LeaveTypeId = LeaveTypes.Earned, Count = 8 },
            }},
            new LeavePolicy() { Name = "After 2 Years" , Details = new List<LeavePolicyDetail> {
                new LeavePolicyDetail() { LeaveTypeId = LeaveTypes.Casual, Count = 8 },
                new LeavePolicyDetail() { LeaveTypeId = LeaveTypes.Sick, Count = 8 },
                new LeavePolicyDetail() { LeaveTypeId = LeaveTypes.Earned, Count = 14 },
            }},
        };

        private static CalendarDay[] GetCalendarDays(Setting setting, Calendar[] calendars)
        {
            var days = new List<CalendarDay>();

            foreach (var calendar in calendars)
            {
                days.AddRange(calendar.MakeWeekends(setting.Weekends));
            }

            return days.ToArray();
        }

        private static readonly AttendanceType[] AttendanceTypeList = new AttendanceType[]
        {
            new AttendanceType() { Id = AttendanceTypes.Full, Code = "F", Name = "Full", IsDefault = true },
            new AttendanceType() { Id = AttendanceTypes.Late, Code = "T", Name = "Late", IsDefault = true },
            new AttendanceType() { Id = AttendanceTypes.Half, Code = "H", Name = "Half", IsDefault = true },
            new AttendanceType() { Id = AttendanceTypes.Leave, Code = "L", Name = "Leave", IsDefault = true },
        };

        private static readonly LeaveType[] LeaveTypeList = new LeaveType[]
        {
            new LeaveType() { Id = LeaveTypes.Casual, Code = "C", Name = "Casual", IsDefault = true },
            new LeaveType() { Id = LeaveTypes.Sick, Code = "S", Name = "Sick", IsDefault = true },
            new LeaveType() { Id = LeaveTypes.Earned, Code = "E", Name = "Earned", IsDefault = true },
            new LeaveType() { Id = LeaveTypes.Paid, Code = "P", Name = "Paid", IsDefault = true },
        };

        private static readonly Employee[] EmployeeList = new Employee[]
        {
            new Employee() { FirstName = "Zeeshan", LastName = "Khatri", Email = "zeeshan.khatri@gmail.com", Joining = new DateTime(2020, 06, 07) },
            new Employee() { FirstName = "Danish", LastName = "Khatri", Email = "danish.khatri@gmail.com", Joining = new DateTime(2020, 07, 07) },
            new Employee() { FirstName = "Shadab", LastName = "Khatri", Email = "shadab.khatri@gmail.com", Joining = new DateTime(2020, 08, 07) },
            new Employee() { FirstName = "Nasir", LastName = "Khatri", Email = "nasir.khatri@gmail.com", Joining = new DateTime(2020, 11, 07) },
        };

        public static async Task RunAsync(ApplicationDbContext context)
        {
            await context.AddAsync(Setting);
            await context.AddRangeAsync(DayTypeList);
            await context.AddRangeAsync(AttendanceTypeList);
            await context.AddRangeAsync(LeaveTypeList);
            foreach(var policy in LeavePolicyList)
            {
                context.SetId(policy);
                context.SetId(policy.Details);
            }
            await context.AddRangeAsync(context.SetId(LeavePolicyList));
            await context.AddRangeAsync(context.SetId(CalendarList));
            await context.AddRangeAsync(context.SetId(GetCalendarDays(Setting, CalendarList)));
            await context.AddRangeAsync(context.SetId(EmployeeList));
            await context.SaveChangesAsync();
        }
    }
}
