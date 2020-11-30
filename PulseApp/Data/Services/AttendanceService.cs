using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        public AttendanceService(IServiceProvider provider, IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.DbFactory = dbFactory;
            this.Provider = provider;
        }

        public IDbContextFactory<ApplicationDbContext> DbFactory { get; set; }
        public IServiceProvider Provider { get; set; }

        public async Task<AttendanceType[]> GetAttendanceTypesAsync()
        {
            using var context = DbFactory.CreateDbContext();

            return await context.AttendanceTypes
                .ToArrayAsync();
        }

        public async Task<LeaveType[]> GetLeaveTypesAsync()
        {
            using var context = DbFactory.CreateDbContext();

            return await context.LeaveTypes
                .ToArrayAsync();
        }

        public async Task<MonthAttendanceDto[]> GetMonthAttendanceAsync(int year, int month)
        {
            var (start, end) = DateTimeHelper.MonthRange(year, month);
            using var context = DbFactory.CreateDbContext();

            var employees = await context.Employees
                .Where(e => e.Joining <= end)
                .Select(EmployeeDto.Selector)
                .ToArrayAsync();

            var attendances = await context.Attendances
                .Where(a => a.Date >= start && a.Date <= end)
                .Select(AttendanceDto.Selector)
                .ToArrayAsync();

            var result = new List<MonthAttendanceDto>();
            result.Fill(employees, attendances, start);
            return result.ToArray();
        }

        public async Task<EmployeeAttendanceDto[]> GetYearAttendanceAsync(int employeeId, int year)
        {
            var (start, end) = DateTimeHelper.YearRange(year);

            using var context = DbFactory.CreateDbContext();

            var employee = await context.Employees
                .Where(e => e.Id == employeeId && e.Joining <= end)
                .Select(EmployeeDto.Selector)
                .SingleOrDefaultAsync();

            if(employee == null)
            {
                return Array.Empty<EmployeeAttendanceDto>();
            }

            if (employee.Joining > start)
            {
                start = employee.Joining;
            }

            var attendances = await context.Attendances
                    .Where(a => a.EmployeeId == employeeId && a.Date >= start && a.Date <= end)
                    .Select(AttendanceDto.Selector)
                    .ToArrayAsync();

            var offDates = await GetOffDates(start, end);

            var result = new List<EmployeeAttendanceDto>();
            result.Fill(employee, attendances, start, offDates);
            return result.ToArray();
        }

        private async Task<DateTime[]> GetOffDates(DateTime start, DateTime end)
        {
            var service = Provider.GetRequiredService<CalendarService>();
            return await service.GetOffDates(start, end);
        }

        public async Task MarkAttendanceAsync(int employeeId, int year, int month, int day, 
            int attendanceTypeId = AttendanceTypes.Full, int? leaveTypeId = null, string comments = null)
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
                    AttendanceTypeId = attendanceTypeId,
                    Comments = comments,
                };

                attendance.SetLeaveTypeId(leaveTypeId);
                context.SetId(attendance);
                await context.Attendances.AddAsync(attendance);
            }
            else
            {
                attendance.AttendanceTypeId = attendanceTypeId;
                attendance.SetLeaveTypeId(leaveTypeId);
                context.Attendances.Update(attendance);
            }

            await context.SaveChangesAsync();
        }
    }

    public class AttendanceDto
    {
        public int EmployeeId { get; set; }

        public int Day { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public int AttendanceTypeId { get; set; }

        public string AttendanceTypeCode { get; set; }

        public int? LeaveTypeId { get; set; }

        public string LeaveTypeCode { get; set; }

        public string Comments { get; set; }

        public static Expression<Func<Attendance, AttendanceDto>> Selector = e => new AttendanceDto
        {
            EmployeeId = e.EmployeeId,
            Day = e.Date.Day,
            Month = e.Date.Month,
            AttendanceTypeId = e.AttendanceTypeId,
            AttendanceTypeCode = e.AttendanceType.Code,
            LeaveTypeId = e.LeaveTypeId,
            LeaveTypeCode = e.LeaveType.Code,
            Comments = e.Comments,
        };
    }

    public class MonthAttendanceDto
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int StartDay { get; set; }
        public Dictionary<int, DayAttendanceDetailDto> Attendance { get; set; }
    }

    public class EmployeeAttendanceDto
    {
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int StartDay { get; set; }
        public int[] OffDays { get; set; }
        public int Days { get; set; }
        public Dictionary<int, DayAttendanceDetailDto> Attendance { get; set; }
    }

    public class DayAttendanceDetailDto
    {
        public int AttendanceTypeId { get; set; }
        public string AttendanceTypeCode { get; set; }
        public int? LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; }
        public string Comments { get; set; }
    }

    public static class AttendanceExtensions
    {
        public static void Fill(this List<MonthAttendanceDto> destination, EmployeeDto[] employees, AttendanceDto[] attendances, DateTime start)
        {
            foreach(var employee in employees)
            {
                var dto = new MonthAttendanceDto()
                {
                    EmployeeId = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    StartDay = employee.Joining < start ? 1 : employee.Joining.Day,
                    Attendance = new Dictionary<int, DayAttendanceDetailDto>(),
                };

                for(int i = 0; i <= 31; i++)
                {
                    var day = attendances.FirstOrDefault(a => a.EmployeeId == employee.Id && a.Day == i);
                    if(day != null)
                    {
                        dto.Attendance.Add(i, new DayAttendanceDetailDto() { 
                            AttendanceTypeId = day.AttendanceTypeId, 
                            AttendanceTypeCode = day.AttendanceTypeCode,
                            LeaveTypeId = day.LeaveTypeId,
                            LeaveTypeCode = day.LeaveTypeCode,
                            Comments = day.Comments 
                        });
                    }
                }

                destination.Add(dto);
            }
        }

        public static void Fill(this List<EmployeeAttendanceDto> destination, EmployeeDto employee, AttendanceDto[] attendances, DateTime start, DateTime[] offDates)
        {
            for(int month = start.Month; month <= 12; month++)
            {
                var firstDay = DateTimeHelper.FirstDay(start.Year, month);
                var dto = new EmployeeAttendanceDto()
                {
                    Month = month,
                    MonthName = DateTimeHelper.GetMonthName(month),
                    Days = DateTime.DaysInMonth(start.Year, month),
                    OffDays = DateTimeHelper.OffDaysOfMonth(offDates, start.Year, month),
                    StartDay = employee.Joining < firstDay ? 1 : employee.Joining.Day,
                    Attendance = new Dictionary<int, DayAttendanceDetailDto>(),
                };

                for (int i = 0; i <= 31; i++)
                {
                    var day = attendances.FirstOrDefault(a => a.Month == month && a.Day == i);
                    if (day != null)
                    {
                        dto.Attendance.Add(i, new DayAttendanceDetailDto()
                        {
                            AttendanceTypeId = day.AttendanceTypeId,
                            AttendanceTypeCode = day.AttendanceTypeCode,
                            LeaveTypeId = day.LeaveTypeId,
                            LeaveTypeCode = day.LeaveTypeCode,
                            Comments = day.Comments
                        });
                    }
                }

                destination.Add(dto);
            }
        }

        public static void SetLeaveTypeId(this Attendance attendance, int? leaveTypeId)
        {
            attendance.LeaveTypeId = null;

            if (attendance.AttendanceTypeId == AttendanceTypes.Leave)
            {
                if(leaveTypeId == null)
                {
                    throw new Exception("Leave Type is not specified.");
                }

                attendance.LeaveTypeId = leaveTypeId;
            }
            
        }
    }
}
