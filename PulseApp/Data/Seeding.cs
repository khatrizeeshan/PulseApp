using PulseApp.Constants;
using PulseApp.Helpers;
using System;
using System.Threading.Tasks;

namespace PulseApp.Data
{
    public class Seeding
    {
        private static readonly AttendanceType[] AttendanceTypeList = new AttendanceType[]
{
            new AttendanceType() { Id = AttendanceTypes.Full, Code = "FD", Name = "Full Day", IsDefault = true },
            new AttendanceType() { Id = AttendanceTypes.Late, Code = "LT", Name = "Late", IsDefault = true },
            new AttendanceType() { Id = AttendanceTypes.Half, Code = "HD", Name = "Half Day", IsDefault = true },
            new AttendanceType() { Id = AttendanceTypes.LeaveCasual, Code = "CL", Name = "Casual Leave", IsDefault = true },
            new AttendanceType() { Id = AttendanceTypes.LeaveSick, Code = "SL", Name = "Sick Leave", IsDefault = true },
            new AttendanceType() { Id = AttendanceTypes.LeaveAnnual, Code = "AL", Name = "Annual Leave", IsDefault = true },
            new AttendanceType() { Id = AttendanceTypes.LeavePaid, Code = "PL", Name = "Paid Leave", IsDefault = true },
            new AttendanceType() { Id = AttendanceTypes.LeaveUnpaid, Code = "UL", Name = "Unpaid Leave", IsDefault = true },
        };

        private static readonly Employee[] EmployeeList = new Employee[]
        {
            new Employee() { FirstName = "Zeeshan", LastName = "Khatri", Email = "zeeshan.khatri@gmail.com", Joining = new DateTime(2020, 06, 07) },
            new Employee() { FirstName = "Danish", LastName = "Khatri", Email = "danish.khatri@gmail.com", Joining = new DateTime(2020, 07, 07) },
            new Employee() { FirstName = "Shadab", LastName = "Khatri", Email = "shadab.khatri@gmail.com", Joining = new DateTime(2020, 08, 07) },
            new Employee() { FirstName = "Nasir", LastName = "Khatri", Email = "nasir.khatri@gmail.com", Joining = new DateTime(2020, 11, 07) },
        };

        public static async Task Run(ApplicationDbContext context)
        {
            await context.AddRangeAsync(AttendanceTypeList);
            await context.AddRangeAsync(context.SetId(EmployeeList));
            await context.SaveChangesAsync();
        }
    }
}
