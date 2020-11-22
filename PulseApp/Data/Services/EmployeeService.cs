using Microsoft.EntityFrameworkCore;
using PulseApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PulseApp.Data
{
    public class EmployeeService
    {

        public EmployeeService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.DbFactory = dbFactory;
        }

        public IDbContextFactory<ApplicationDbContext> DbFactory { get; set; }

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
            employee.Id = context.GetSequence<Employee>();
            await context.Employees.AddAsync(employee);
            await context.SaveChangesAsync();

            return employee.Id;
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
