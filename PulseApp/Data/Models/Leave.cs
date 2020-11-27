using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace PulseApp.Data
{
    public class Leave : BaseModel<int>
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public int CalendarId { get; set; }
        public Calendar Calendar { get; set; }
        public DateTime Date { get; set; }
        public LeaveType Type { get; set; }
        public int Count { get; set; }
    }

    public class LeaveType : BaseModel<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
    }

    public class LeaveConfiguration : BaseEntityTypeConfiguration<Leave> { }
    public class LeaveTypeConfiguration : BaseEntityTypeConfiguration<LeaveType> { }
}

