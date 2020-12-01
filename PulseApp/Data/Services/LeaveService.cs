using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PulseApp.Data
{
    public class LeaveService
    {

        public LeaveService(IServiceProvider provider, IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.Provider = provider;
            this.DbFactory = dbFactory;
        }

        public IDbContextFactory<ApplicationDbContext> DbFactory { get; set; }

        public IServiceProvider Provider { get; set; }

        public async Task<LeaveDto[]> GetLeaveLedger(int employeeId, int calendarId)
        {
            using var context = DbFactory.CreateDbContext();

            return await context.Leaves
                .Where(l => l.EmployeeId == employeeId &&
                            l.CalendarId == calendarId)
                .Select(LeaveDto.Selector)
                .ToArrayAsync();
        }
    }

    public class LeaveDto
    {
        public DateTime Date { get; set; }
        public int LeaveTypeId { get; set; }
        public string LeaveType { get; set; }
        public bool Opening { get; set; }
        public bool Forwarded { get; set; }
        public int Count { get; set; }

        public static Expression<Func<Leave, LeaveDto>> Selector = e => new LeaveDto
        {
            Date = e.Date,
            LeaveTypeId = e.LeaveTypeId,
            LeaveType = e.LeaveType.Name,
            Opening = e.Opening,
            Forwarded = e.Forwarded,
            Count = e.Count,
        };
    }
}
