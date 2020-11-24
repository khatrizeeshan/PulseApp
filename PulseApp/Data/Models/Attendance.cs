using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace PulseApp.Data
{
    public class Attendance : BaseModel<int>
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public DateTime Date { get; set; }
        public int AttendanceTypeId { get; set; }
        public AttendanceType AttendanceType { get; set; }
        public int? LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; }
        public int Hours { get; set; }
        public string Comments { get; set; }

    }

    public class AttendanceConfiguration : BaseEntityTypeConfiguration, IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            base.Configure(builder);
        }
    }
}
