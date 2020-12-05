using Microsoft.EntityFrameworkCore;
using PulseApp.Data;
using PulseApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PulseApp.Protos;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

namespace PulseApp.Services
{
    public class LeaveService : LeaveManager.LeaveManagerBase
    {

        public LeaveService(IServiceProvider provider, IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            this.Provider = provider;
            this.DbFactory = dbFactory;
        }

        public IDbContextFactory<ApplicationDbContext> DbFactory { get; set; }

        public IServiceProvider Provider { get; set; }

        public override async Task<LeaveLedgerResponse> GetLeaveLedger(LeaveLedgerRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();

            var types = await db.LeaveTypes
                .Select(LeaveTypeDto.Selector)
                .ToArrayAsync();

            var leaves = await db.Leaves
                .Where(l => l.EmployeeId == request.EmployeeId &&
                            l.CalendarId == request.CalendarId)
                .Select(LeaveDto.Selector)
                .ToArrayAsync();

            var response = new LeaveLedgerResponse();
            response.LeaveTypes.AddRange(types);

            foreach (var leave in leaves.Where(l => l.Forwarded)) {
                response.Forward.Add(leave.LeaveTypeId, leave.Count);
            }

            foreach (var leave in leaves.Where(l => l.Opening)) {
                response.Opening.Add(leave.LeaveTypeId, leave.Count);
            }

            foreach (var leave in leaves.Where(l => !l.Opening && !l.Forwarded).OrderBy(l => l.Date))
            {
                var item = new LeaveDateProto();
                item.Date = Timestamp.FromDateTime(leave.Date);
                item.LeaveTypeCount.Add(leave.LeaveTypeId, leave.Count);

                response.Leaves.Add(item);
            }

            return response;
        }

        public override async Task<LeaveTypeResponse> GetLeaveTypes(LeaveTypeRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();

            var types = await db.LeaveTypes
                .Select(LeaveTypeDto.Selector)
                .ToArrayAsync();

            var response = new LeaveTypeResponse();
            response.LeaveTypes.Add(types);

            return response;
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

    public class LeaveTypeDto
    {
        public static Expression<Func<LeaveType, LeaveTypeProto>> Selector = e => new LeaveTypeProto
        {
            Id = e.Id,
            Name = e.Name,
        };

    }

}
