using Microsoft.EntityFrameworkCore;
using PulseApp.Data;
using PulseApp.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PulseApp.Protos;
using PulseApp.Helpers;
using Grpc.Core;


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

        public override async Task<LeavePoliciesResponse> GetLeavePolicies(EmptyRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();

            var policies = await db.LeavePolicies
                .Select(LeavePoliciesDto.ListSelector)
                .ToArrayAsync();

            var response = new LeavePoliciesResponse();
            response.LeavePolicies.Add(policies);

            return response;
        }

        public override async Task<LeavePolicyResponse> GetLeavePolicy(IdRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();
            var leavePolicy = await db.LeavePolicies
                    .Select(LeavePoliciesDto.Selector)
                    .SingleOrDefaultAsync(e => e.Id == request.Id);

            var details = await db.LeavePolicyDetails
                    .Select(LeavePoliciesDto.DetailSelector)
                    .SingleOrDefaultAsync(e => e.Id == request.Id);

            leavePolicy.LeavePolicyDetails.Add(details);
            
            var response = new LeavePolicyResponse();
            response.LeavePolicy = leavePolicy;
            return response;
        }

        public override async Task<EmptyResponse> DeleteLeavePolicy(IdRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();
            db.LeavePolicies.Remove(new LeavePolicy() { Id = request.Id });
            await db.SaveChangesAsync();

            return new EmptyResponse();
        }

        public override async Task<IdResponse> AddLeavePolicy(LeavePolicyAddRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();

            var leavePolicy = new LeavePolicy
            {
                Name = request.Name,
                LeavePolicyTypeId = request.LeavePolicyTypeId,
                Details = request.LeavePolicyDetails
                    .Select(d => new LeavePolicyDetail() { LeaveTypeId = d.LeaveTypeId, Count = d.Count, Forwardable = d.Forwardable }).ToArray()
                
            };

            db.SetId(leavePolicy);
            db.SetId(leavePolicy.Details);

            await db.LeavePolicies.AddAsync(leavePolicy);
            await db.SaveChangesAsync();

            return new IdResponse() { Id = leavePolicy.Id };
        }

        public override Task<EmptyResponse> UpdateLeavePolicy(LeavePolicyUpdateRequest request, ServerCallContext context)
        {
            return base.UpdateLeavePolicy(request, context);
        }

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
                item.Date = leave.Date.ToDate();
                item.LeaveTypeCount.Add(leave.LeaveTypeId, leave.Count);

                response.Leaves.Add(item);
            }

            return response;
        }

        public override async Task<LeaveTypesResponse> GetLeaveTypes(EmptyRequest request, ServerCallContext context)
        {
            using var db = DbFactory.CreateDbContext();

            var types = await db.LeaveTypes
                .Select(LeaveTypeDto.Selector)
                .ToArrayAsync();

            var response = new LeaveTypesResponse();
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

    public class LeavePoliciesDto
    {
        public static Expression<Func<LeavePolicy, LeavePolicyListProto>> ListSelector = e => new LeavePolicyListProto
        {
            Id = e.Id,
            Name = e.Name,
            LeavePolicyTypeId = e.LeavePolicyTypeId,
            LeavePolicyTypeName = e.LeavePolicyType.Name,
        };

        public static Expression<Func<LeavePolicy, LeavePolicyProto>> Selector = e => new LeavePolicyProto
        {
            Id = e.Id,
            Name = e.Name,
            LeavePolicyTypeId = e.LeavePolicyTypeId,
            LeavePolicyTypeName = e.LeavePolicyType.Name,
        };

        public static Expression<Func<LeavePolicyDetail, LeavePolicyDetailProto>> DetailSelector = e => new LeavePolicyDetailProto
        {
            Id = e.Id,
            LeaveTypeId = e.LeaveTypeId,
            LeaveTypeName = e.LeaveType.Name,
            Count = e.Count,
            Forwardable = e.Forwardable,
        };
    }

    public class LeaveTypeDto
    {
        public static Expression<Func<LeaveType, LeaveTypeProto>> Selector = e => new LeaveTypeProto
        {
            Id = e.Id,
            Code = e.Code,
            Name = e.Name,
        };

    }
}
