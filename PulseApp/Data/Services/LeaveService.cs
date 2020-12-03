using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        public async Task<LeaveLedgerDto> GetLeaveLedger(int employeeId, int calendarId)
        {
            using var context = DbFactory.CreateDbContext();

            var types = await context.LeaveTypes
                .Select(LeaveTypeDto.Selector)
                .ToArrayAsync();

            var leaves = await context.Leaves
                .Where(l => l.EmployeeId == employeeId &&
                            l.CalendarId == calendarId)
                .Select(LeaveDto.Selector)
                .ToArrayAsync();

            var ledger = new LeaveLedgerDto
            {
                LeaveTypes = types,
                Farward = new Dictionary<int, int>(),
                Opening = new Dictionary<int, int>(),
                Leaves = new Dictionary<DateTime, Dictionary<int, int>>()
            };

            foreach (var leave in leaves.Where(l => l.Forwarded)) {
               ledger.Farward.Add(leave.LeaveTypeId, leave.Count);
            }

            foreach (var leave in leaves.Where(l => l.Opening)) {
                ledger.Opening.Add(leave.LeaveTypeId, leave.Count);
            }

            foreach (var leave in leaves.Where(l => !l.Opening && !l.Forwarded).OrderBy(l => l.Date))
            {
                var dictionary = new Dictionary<int, int>
                {
                    { leave.LeaveTypeId, leave.Count }
                };
                ledger.Leaves.Add(leave.Date, dictionary);
            }

            return ledger;
        }
    }

    public class LeaveDto
    {
        public DateTime Date { get; set; }
        public int LeaveTypeId { get; set; }
        public string LeaveType { get; set; }
        public bool Forwarded { get; set; }
        public bool Opening { get; set; }
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

    public class LeaveLedgerDto
    {
        public LeaveTypeDto[] LeaveTypes { get; set; }

        public Dictionary<int, int> Farward { get; set; }

        public Dictionary<int, int> Opening { get; set; }

        public Dictionary<DateTime, Dictionary<int, int>> Leaves { get; set; }

    }

    public class LeaveTypeDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public static Expression<Func<LeaveType, LeaveTypeDto>> Selector = e => new LeaveTypeDto
        {
            Id = e.Id,
            Name = e.Name,
        };

    }

}
