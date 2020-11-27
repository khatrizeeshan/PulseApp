using PulseApp.Constants;
using PulseApp.Helpers;
using System;
using System.Threading.Tasks;

namespace PulseApp.Data
{
    public class Seeding
    {
        private static readonly Setting Setting = new Setting() { Id = 0, WeekDays = "0111110" };

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
            new LeaveType() { Id = LeaveTypes.Earned, Code = "E", Name = "Annual", IsDefault = true },
            new LeaveType() { Id = LeaveTypes.Paid, Code = "D", Name = "Paid", IsDefault = true },
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
            await context.AddAsync(Setting);
            await context.AddRangeAsync(AttendanceTypeList);
            await context.AddRangeAsync(LeaveTypeList);
            await context.AddRangeAsync(context.SetId(EmployeeList));
            await context.SaveChangesAsync();
        }
    }
}
