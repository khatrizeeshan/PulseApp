using System;
using System.Collections.Generic;

namespace PulseApp.Data
{
    public class LeavePolicy : BaseModel<int>
    {
        public string Name { get; set; }

        public ICollection<LeavePolicyDetail> Details { get; set; }
    }

    public class LeavePolicyDetail : BaseModel<int>
    {
        public int LeavePolicyId { get; set; }

        public LeavePolicy LeavePolicy { get; set; }

        public int LeaveTypeId { get; set; }

        public LeaveType LeaveType { get; set; }

        public int Count { get; set; }
    }

    public class Leave : BaseModel<int>
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public int CalendarId { get; set; }
        public Calendar Calendar { get; set; }
        public DateTime Date { get; set; }
        public int LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; }
        public int Count { get; set; }
    }

    public class LeaveType : BaseModel<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
    }

    public class LeavePolicyConfiguration : BaseEntityTypeConfiguration<LeavePolicy> { }
    public class LeavePolicyDetailConfiguration : BaseEntityTypeConfiguration<LeavePolicyDetail> { }
    public class LeaveConfiguration : BaseEntityTypeConfiguration<Leave> { }
    public class LeaveTypeConfiguration : BaseEntityTypeConfiguration<LeaveType> { }
}

