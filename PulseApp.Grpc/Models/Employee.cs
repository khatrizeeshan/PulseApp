using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace PulseApp.Models
{
    public class Employee : BaseModel<int>
    {
        public int? UserId { get; set; }
        public User User { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime Joining { get; set; }
        public ICollection<Timing> Timings { get; set; }
        public ICollection<Attendance> Attendances { get; set; }
    }

    public class EmployeeCalendar : BaseModel<int>
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public int CalendarId { get; set; }
        public Calendar Calendar { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class EmployeeLeavePolicy : BaseModel<int>
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public int LeavePolicyId { get; set; }
        public LeavePolicy LeavePolicy { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class EmployeeConfiguration : BaseEntityTypeConfiguration<Employee> { }
    public class EmployeeCalendarConfiguration : BaseEntityTypeConfiguration<EmployeeCalendar> { }
    public class EmployeeLeavePolicyConfiguration : BaseEntityTypeConfiguration<EmployeeLeavePolicy> { }

}
