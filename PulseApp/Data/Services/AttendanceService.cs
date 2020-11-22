using Microsoft.EntityFrameworkCore;
using PulseApp.Constants;
using PulseApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PulseApp.Data
{
    public class AttendanceService
    {

        public AttendanceService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.DbFactory = dbFactory;
        }

        public IDbContextFactory<ApplicationDbContext> DbFactory { get; set; }

        public async Task<AttendanceType[]> GetAttenanceTypesAsync()
        {
            using var context = DbFactory.CreateDbContext();

            return await context.AttendanceTypes
                .ToArrayAsync();
        }

        public async Task<MonthAttendanceDto[]> GetMonthAttendanceAsync(int year, int month)
        {
            var monthEnd = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            using var context = DbFactory.CreateDbContext();

            var employees = await context.Employees
                .Where(e => e.Joining <= monthEnd)
                .Select(EmployeeDto.Selector)
                .ToArrayAsync();

            var attendances = await context.Attendances
                    .Where(a => a.Date.Month == month && a.Date.Year == year)
                    .Select(AttendanceDto.Selector)
                    .ToArrayAsync();

            var result = new List<MonthAttendanceDto>();
            result.Fill(employees, attendances);
            return result.ToArray();
        }

        public async Task MarkAttendanceAsync(int employeeId, int year, int month, int day, int typeId = AttendanceTypes.Full, string comments = null)
        {
            var date = new DateTime(year, month, day);
            using var context = DbFactory.CreateDbContext();

            var attendance = await context.Attendances.SingleOrDefaultAsync(e => e.EmployeeId == employeeId && e.Date == date);
            if(attendance == null)
            {
                attendance = new Attendance()
                {
                    EmployeeId = employeeId,
                    Date = date,
                    TypeId = typeId,
                    Comments = comments,
                };
                context.SetId(attendance);
                await context.Attendances.AddAsync(attendance);
            }
            else
            {
                attendance.TypeId = typeId;
                context.Attendances.Update(attendance);
            }

            await context.SaveChangesAsync();
        }
    }

    public class AttendanceDto
    {
        public int EmployeeId { get; set; }

        public int Day { get; set; }

        public int TypeId { get; set; }

        public string TypeCode { get; set; }

        public string Comments { get; set; }

        public static Expression<Func<Attendance, AttendanceDto>> Selector = e => new AttendanceDto
        {
            EmployeeId = e.EmployeeId,
            Day = e.Date.Day,
            TypeId = e.TypeId,
            TypeCode = e.Type.Code,
            Comments = e.Comments,
        };
    }

    public class MonthAttendanceDto
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Dictionary<int, TypeCodeCommentsDto> Attendance { get; set; }
    }

    public class TypeCodeCommentsDto
    {
        public int TypeId { get; set; }
        public string TypeCode { get; set; }
        public string Comments { get; set; }
    }

    public static class AttendanceExtensions
    {
        public static void Fill(this List<MonthAttendanceDto> destination, EmployeeDto[] employees, AttendanceDto[] attendances)
        {
            foreach(var employee in employees)
            {
                var month = new MonthAttendanceDto()
                {
                    EmployeeId = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Attendance = new Dictionary<int, TypeCodeCommentsDto>(),
                };

                for(int i = 0; i <= 31; i++)
                {
                    var day = attendances.FirstOrDefault(a => a.EmployeeId == employee.Id && a.Day == i);
                    if(day != null)
                    {
                        month.Attendance.Add(i, new TypeCodeCommentsDto() { TypeId = day.TypeId, TypeCode = day.TypeCode, Comments = day.Comments });
                    }
                }

                destination.Add(month);
            }
        }
    }
}
