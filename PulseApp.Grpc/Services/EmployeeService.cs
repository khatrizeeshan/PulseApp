using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    public class EmployeeService : EmployeeManager.EmployeeManagerBase
    {
        public EmployeeService(IServiceProvider provider, IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.DbFactory = dbFactory;
            this.Provider = provider;
        }

        public IDbContextFactory<ApplicationDbContext> DbFactory { get; set; }
        public IServiceProvider Provider { get; set; }

        public override async Task<EmployeesResponse> GetEmployees(EmptyRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();
            var employees = await db.Employees
                    .Select(EmployeeDto.Selector)
                    .ToArrayAsync();

            var response = new EmployeesResponse();
            response.Employees.Add(employees);

            return response;
        }


        public override async Task<EmployeeResponse> GetEmployee(IdRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();
            var employee = await db.Employees
                .Select(EmployeeDto.Selector)
                .SingleOrDefaultAsync(e => e.Id == request.Id);

            var calendars = await db.EmployeeCalendars
                .Where(e => e.EmployeeId == request.Id)
                .Select(EmployeeCalendarDto.Selector)
                .ToArrayAsync();

            employee.Calendars.Add(calendars);

            var policies = await db.EmployeeLeavePolicies
                .Where(e => e.EmployeeId == request.Id)
                .Select(EmployeeLeavePolicyDto.Selector)
                .ToArrayAsync();

            employee.LeavePolicies.Add(policies);

            return new EmployeeResponse() { Employee = employee };
        }

        public override async Task<EmptyResponse> DeleteEmployee(IdRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();
            db.Employees.Remove(new Employee() { Id = request.Id });
            await db.SaveChangesAsync();

            return new EmptyResponse();
        }

        public override async Task<EmptyResponse> UpdateEmployee(EmployeeUpdateRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();
            var existing = await db.Employees.SingleOrDefaultAsync(e => e.Id == request.Id);
            existing.FirstName = request.FirstName;
            existing.LastName = request.LastName;
            existing.Email = request.Email;
            existing.Joining = request.Joining.ToDateTime();
            db.Employees.Update(existing);
            await db.SaveChangesAsync();

            return new EmptyResponse();
        }

        public override async Task<IdResponse> AddEmployee(EmployeeAddRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();
            var employee = new Employee();
            employee.FirstName = request.FirstName;
            employee.LastName = request.LastName;
            employee.Email = request.Email;
            employee.Joining = request.Joining.ToDateTime();
            db.SetId(employee);

            var calendar = new EmployeeCalendar() { EmployeeId = employee.Id, CalendarId = request.CalendarId, StartDate = employee.Joining };
            db.SetId(calendar);

            var policy = new EmployeeLeavePolicy() { EmployeeId = employee.Id, LeavePolicyId = request.LeavePolicyId, StartDate = employee.Joining };
            db.SetId(policy);

            await db.Employees.AddAsync(employee);
            await db.EmployeeCalendars.AddAsync(calendar);
            await db.EmployeeLeavePolicies.AddAsync(policy);
            await db.SaveChangesAsync();

            return new IdResponse { Id = employee.Id };
        }

        internal async Task<int> GetCalendarIdAsync(int employeeId, DateTime date)
        {
            using var db = DbFactory.CreateDbContext();
            var calendarId = await db.EmployeeCalendars.Where(c => c.EmployeeId == employeeId && c.StartDate <= date)
                                        .OrderByDescending(c => c.CalendarId)
                                        .Select(c => c.CalendarId)
                                        .FirstOrDefaultAsync();

            if (calendarId == 0)
            {
                throw new Exception("No calendar found for selected employee.");
            }


            return calendarId;
        }

        internal async Task<int> GetLeavePolicyIdAsync(int employeeId, DateTime date)
        {
            using var db = DbFactory.CreateDbContext();
            var leavePolicyId = await db.EmployeeLeavePolicies.Where(c => c.EmployeeId == employeeId && c.StartDate >= date)
                                        .Select(c => c.LeavePolicyId)
                                        .SingleOrDefaultAsync();

            if (leavePolicyId == 0)
            {
                throw new Exception("No leave policy found for selected employee.");
            }


            return leavePolicyId;
        }

    }

    public class EmployeeDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime Joining { get; set; }

        public static Expression<Func<Employee, EmployeeDto>> SelectorDto = e => new EmployeeDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            Joining = e.Joining,
        };

        public static Expression<Func<Employee, EmployeeProto>> Selector = e => new EmployeeProto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            Joining = e.Joining.ToDate(),
        };
    }

    public class EmployeeCalendarDto
    {
        public static Expression<Func<EmployeeCalendar, EmployeeCalendarProto>> Selector = e => new EmployeeCalendarProto
        {
            Id = e.Id,
            CalendarId = e.CalendarId,
            CalendarName = e.Calendar.Name,
            StartDate = e.StartDate.ToDate(),
        };
    }

    public class EmployeeLeavePolicyDto
    {
        public static Expression<Func<EmployeeLeavePolicy, EmployeeLeavePolicyProto>> Selector = e => new EmployeeLeavePolicyProto
        {
            Id = e.Id,
            LeavePolicyId = e.LeavePolicyId,
            LeavePolicyName = e.LeavePolicy.Name,
            StartDate = e.StartDate.ToDate(),
        };
    }

    public class EmployeeJoiningDto
    {
        public int Id { get; set; }
        public DateTime Joining { get; set; }

        public static Expression<Func<Employee, EmployeeJoiningDto>> Selector = e => new EmployeeJoiningDto
        {
            Id = e.Id,
            Joining = e.Joining,
        };
    }

    public static class EmployeeExtensions
    {
        public static void Fill(this Employee employee, EmployeeDto dto)
        {
            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.Email = dto.Email;
            employee.Joining = dto.Joining;
        }
    }
}
