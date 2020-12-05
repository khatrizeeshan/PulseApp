using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PulseApp.Constants;
using PulseApp.Data;
using PulseApp.Helpers;
using PulseApp.Models;
using PulseApp.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PulseApp.Services
{
    public class AttendanceService : AttendanceManager.AttendanceManagerBase
    {

        public AttendanceService(IServiceProvider provider, IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.DbFactory = dbFactory;
            this.Provider = provider;
        }

        public IDbContextFactory<ApplicationDbContext> DbFactory { get; set; }

        public IServiceProvider Provider { get; set; }

        public override async Task<AttendanceTypeResponse> GetAttendanceTypes(AttendanceTypeRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();

            var types = await db.AttendanceTypes
                .Select(AttendanceTypeDto.Selector)
                .ToArrayAsync();

            var response = new AttendanceTypeResponse();
            response.AttendanceTypes.Add(types);

            return response;
        }

        public override async Task<MonthAttendanceResponse> GetMonthAttendance(MonthAttendanceRequest request, ServerCallContext context)
        {
            var (start, end) = DateTimeHelper.MonthRange(request.Year, request.Month);
            using var db = DbFactory.CreateDbContext();

            var employees = await db.Employees
                .Where(e => e.Joining <= end)
                .Select(EmployeeDto.Selector)
                .ToArrayAsync();

            var attendances = await db.Attendances
                .Where(a => a.Date >= start && a.Date <= end)
                .Select(AttendanceDto.Selector)
                .ToArrayAsync();

            var response = new MonthAttendanceResponse();
            response.Fill(employees, attendances, start);
            return response;
        }

        public override async Task<EmployeeAttendanceResponse> GetEmployeeAttendance(EmployeeAttendanceRequest request, ServerCallContext context)
        {
            var (start, end) = DateTimeHelper.YearRange(request.Year);

            using var db = DbFactory.CreateDbContext();

            var employee = await db.Employees
                .Where(e => e.Id == request.EmployeeId && e.Joining <= end)
                .Select(EmployeeDto.Selector)
                .SingleOrDefaultAsync();

            var response = new EmployeeAttendanceResponse();
            if (employee == null)
            {
                return response;
            }

            if (employee.Joining > start)
            {
                start = employee.Joining;
            }

            var attendances = await db.Attendances
                    .Where(a => a.EmployeeId == request.EmployeeId && a.Date >= start && a.Date <= end)
                    .Select(AttendanceDto.Selector)
                    .ToArrayAsync();

            var offDates = await GetOffDates(start, end);

            response.Fill(employee, attendances, start, offDates);
            return response;
        }

        public override async Task<MarkAttendanceResponse> MarkAttendance(MarkAttendanceRequest request, ServerCallContext context)
        {
            var date = new DateTime(request.Year, request.Month, request.Day);
            using var db = DbFactory.CreateDbContext();
            var service = Provider.GetRequiredService<EmployeeService>();

            var calendarId = await service.GetCalendarIdAsync(request.EmployeeId, date);

            var attendance = await db.Attendances.SingleOrDefaultAsync(e => e.EmployeeId == request.EmployeeId && e.Date == date);
            if(attendance == null)
            {
                attendance = new Attendance()
                {
                    EmployeeId = request.EmployeeId,
                    Date = date,
                    AttendanceTypeId = request.AttendanceTypeId,
                    Comments = request.Comments,
                };

                attendance.SetLeaveTypeId(request.LeaveTypeId);
                db.SetId(attendance);
                await db.Attendances.AddAsync(attendance);
            }
            else
            {
                if (attendance.AttendanceTypeId == AttendanceTypes.Leave)
                {
                    var leave = await db.Leaves.SingleOrDefaultAsync(e => e.AttendanceId == attendance.Id);
                    if (leave == null) {
                        db.Leaves.Remove(leave);
                    }
                }
                attendance.AttendanceTypeId = request.AttendanceTypeId;
                attendance.SetLeaveTypeId(request.LeaveTypeId);
                db.Attendances.Update(attendance);
            }

            if (attendance.AttendanceTypeId == AttendanceTypes.Leave)
            {
                var leave = new Leave()
                {
                    AttendanceId = attendance.Id,
                    CalendarId = calendarId,
                    LeaveTypeId = attendance.LeaveTypeId.Value,
                    Date = attendance.Date,
                    EmployeeId = attendance.EmployeeId,
                    Count = -1,
                };
                db.SetId(leave);
                await db.Leaves.AddAsync(leave);
            }

            await db.SaveChangesAsync();

            return new MarkAttendanceResponse();
        }

        private async Task<DateTime[]> GetOffDates(DateTime start, DateTime end)
        {
            var service = Provider.GetRequiredService<CalendarService>();
            return await service.GetOffDates(start, end);
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

    //public class MonthAttendanceDto
    //{
    //    public int EmployeeId { get; set; }
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public int StartDay { get; set; }
    //    public Dictionary<int, DayAttendanceDetailDto> Attendance { get; set; }
    //}

    //public class EmployeeAttendanceDto
    //{
    //    public int Month { get; set; }
    //    public string MonthName { get; set; }
    //    public int StartDay { get; set; }
    //    public int[] OffDays { get; set; }
    //    public int Days { get; set; }
    //    public Dictionary<int, DayAttendanceDetailDto> Attendance { get; set; }
    //}

    //public class DayAttendanceDetailDto
    //{
    //    public int AttendanceTypeId { get; set; }
    //    public string AttendanceTypeCode { get; set; }
    //    public int? LeaveTypeId { get; set; }
    //    public string LeaveTypeCode { get; set; }
    //    public string Comments { get; set; }
    //}

    public class AttendanceTypeDto
    {
        public static Expression<Func<AttendanceType, AttendanceTypeProto>> Selector = e => new AttendanceTypeProto
        {
            Id = e.Id,
            Code = e.Code,
            Name = e.Name,
            IsDefault = e.IsDefault
        };

    }

    public static class AttendanceExtensions
    {
        public static void Fill(this MonthAttendanceResponse response, EmployeeDto[] employees, AttendanceDto[] attendances, DateTime start)
        {
            foreach (var each in employees)
            {
                var employee = new EmployeeAttendanceProto()
                {
                    EmployeeId = each.Id,
                    FirstName = each.FirstName,
                    LastName = each.LastName,
                    StartDay = each.Joining < start ? 1 : each.Joining.Day,
                };

                for (int i = 0; i <= 31; i++)
                {
                    var day = attendances.FirstOrDefault(a => a.EmployeeId == each.Id && a.Day == i);
                    if (day != null)
                    {
                        employee.Attendance.Add(i, new DayAttendanceDetailProto()
                        {
                            AttendanceTypeId = day.AttendanceTypeId,
                            AttendanceTypeCode = day.AttendanceTypeCode,
                            LeaveTypeId = day.LeaveTypeId.GetValueOrDefault(),
                            LeaveTypeCode = day.LeaveTypeCode,
                            Comments = day.Comments
                        });
                    }
                }

                response.Employees.Add(employee);
            }
        }

        public static void Fill(this EmployeeAttendanceResponse response, EmployeeDto employee, AttendanceDto[] attendances, DateTime start, DateTime[] offDates)
        {
            for (int current = start.Month; current <= 12; current++)
            {
                var firstDay = DateTimeHelper.FirstDay(start.Year, current);
                var month = new MonthAttendanceProto()
                {
                    Month = current,
                    MonthName = DateTimeHelper.GetMonthName(current),
                    Days = DateTime.DaysInMonth(start.Year, current),
                    StartDay = employee.Joining < firstDay ? 1 : employee.Joining.Day,
                };

                month.OffDays.Add(DateTimeHelper.OffDaysOfMonth(offDates, start.Year, current));


                for (int i = 0; i <= 31; i++)
                {
                    var day = attendances.FirstOrDefault(a => a.Month == current && a.Day == i);
                    if (day != null)
                    {
                        month.Attendance.Add(i, new DayAttendanceDetailProto()
                        {
                            AttendanceTypeId = day.AttendanceTypeId,
                            AttendanceTypeCode = day.AttendanceTypeCode,
                            LeaveTypeId = day.LeaveTypeId.GetValueOrDefault(),
                            LeaveTypeCode = day.LeaveTypeCode,
                            Comments = day.Comments
                        });
                    }
                }

                response.Months.Add(month);
            }
        }

        public static void SetLeaveTypeId(this Attendance attendance, int leaveTypeId)
        {
            attendance.LeaveTypeId = null;

            if (attendance.AttendanceTypeId == AttendanceTypes.Leave)
            {
                if(leaveTypeId == 0)
                {
                    throw new Exception("Leave Type is not specified.");
                }

                attendance.LeaveTypeId = leaveTypeId;
            }
            
        }
    }
}
