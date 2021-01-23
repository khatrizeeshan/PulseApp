using System;
using System.Collections.Generic;

namespace PulseApp.Models
{
    public class LeavePolicyType : BaseModel<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class LeavePolicy : BaseModel<int>
    {
        public string Name { get; set; }
        public int LeavePolicyTypeId { get; set; }
        public LeavePolicyType LeavePolicyType { get; set; }
        public ICollection<LeavePolicyDetail> Details { get; set; }
    }

    public class LeavePolicyDetail : BaseModel<int>
    {
        public int LeavePolicyId { get; set; }
        public LeavePolicy LeavePolicy { get; set; }
        public int LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; }
        public int Count { get; set; }
        public bool Forwardable { get; set; }
    }

    public class Leave : BaseModel<int>
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public int CalendarId { get; set; }
        public Calendar Calendar { get; set; }
        public int? AttendanceId { get; set; }
        public Attendance Attendance { get; set; }
        public DateTime Date { get; set; }
        public int LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; }
        public bool Opening { get; set; }
        public bool Forwarded { get; set; }
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

