using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PulseApp.Data;
using PulseApp.Helpers;
using PulseApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PulseApp.Services
{
    public class EmployeeService
    {

        public EmployeeService(IServiceProvider provider, IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.DbFactory = dbFactory;
            this.Provider = provider;
        }

        public IDbContextFactory<ApplicationDbContext> DbFactory { get; set; }
        public IServiceProvider Provider { get; set; }

        public async Task<EmployeeDto[]> GetEmployeesAsync()
        {
            using var context = DbFactory.CreateDbContext();
            return await context.Employees
                    .Select(EmployeeDto.Selector)
                    .ToArrayAsync();
        }

        public async Task<EmployeeDto> GetEmployeeAsync(int id)
        {
            using var context = DbFactory.CreateDbContext();
            return await context.Employees
                .Select(EmployeeDto.Selector)
                .SingleOrDefaultAsync(e => e.Id == id);
        }

        public async Task DeleteAsync(int id)
        {
            using var context = DbFactory.CreateDbContext();
            context.Employees.Remove(new Employee() { Id = id });
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(int id, EmployeeDto dto)
        {
            using var context = DbFactory.CreateDbContext();
            var existing = await context.Employees.SingleOrDefaultAsync(e => e.Id == id);
            existing.Fill(dto);
            context.Employees.Update(existing);
            await context.SaveChangesAsync();
        }

        public async Task<int> AddAsync(EmployeeDto dto)
        {
            using var context = DbFactory.CreateDbContext();
            var employee = new Employee();
            employee.Fill(dto);
            context.SetId(employee);
            await context.Employees.AddAsync(employee);
            await context.SaveChangesAsync();

            return employee.Id;
        }

        public async Task<int> GetCalendarIdAsync(int employeeId, DateTime date)
        {
            using var context = DbFactory.CreateDbContext();
            var employee = await context.Employees.Where(e => e.Id == employeeId)
                                        .Select(EmployeeJoiningDto.Selector)
                                        .SingleOrDefaultAsync();

            if (date < employee.Joining) {
                throw new Exception("No calendar found for selected employee.");
            }

            var service = Provider.GetRequiredService<CalendarService>();
            return await service.GetCalendarIdAsync(date);
        }

    }

    public class EmployeeDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime Joining { get; set; }

        public static Expression<Func<Employee, EmployeeDto>> Selector = e => new EmployeeDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            Joining = e.Joining,
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
